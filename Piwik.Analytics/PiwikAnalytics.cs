﻿// Copyright (c) Roland Pop All rights reserved.
// Licensed under the BSD 2-clause "Simplified" License. See License.txt in the project root for license information.

using Newtonsoft.Json.Linq;
using Piwik.Analytics.Parameters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Piwik.Analytics
{
    abstract public class PiwikAnalytics
    {
        public static string URL;

        private string tokenAuth;

        abstract protected string getPlugin();

        public void setTokenAuth(string tokenAuth)
        {
            this.tokenAuth = tokenAuth;
        }

        protected T sendRequest<T>(string method, List<Parameter> parameters)
        {
            if (String.IsNullOrEmpty(URL))
            {
                throw new Exception("You must first set the Piwik Server URL by setting the static property 'URL'");
            }

            parameters.Add(new SimpleParameter("token_auth", this.tokenAuth));
            parameters.Add(new SimpleParameter("method", this.getPlugin() + "." + method));

            string url = URL + "/?module=API&format=json";

            foreach (Parameter parameter in parameters)
            {
                url += parameter.serialize();
            }

            HttpWebResponse httpResponse = (HttpWebResponse)((HttpWebRequest)WebRequest.Create(url)).GetResponse();

            string responseData;

            using (StreamReader sr = new StreamReader(httpResponse.GetResponseStream(), System.Text.Encoding.GetEncoding("utf-8")))
            {
                responseData = sr.ReadLine();

                if (String.IsNullOrEmpty(responseData))
                {
                    throw new PiwikAPIException("The server response doesn't contain any data.");
                }

            }

            httpResponse.Close();

            Object deserializedObject;

            if (responseData.StartsWith('['))
            {
                var jObject = JArray.Parse(responseData);
                deserializedObject = jObject.ToObject<T>();
            }
            else
            {
                var jObject = JObject.Parse(responseData);
                deserializedObject = jObject.ToObject<T>();
            }

            if (deserializedObject == null)
            {
                throw new PiwikAPIException(
                    "The server response is not deserializable. " +
                    "Please contact the developer with the following details : responseData = " + responseData
                );
            }

            if (!(deserializedObject is T))
            {
                // Didnt test this 
                if (deserializedObject is Dictionary<string, List<object>>)
                {
                    var result = (Dictionary<string, List<object>>)deserializedObject;
                    string resultString = result["result"][0].ToString();

                    if (resultString.Equals("error"))
                    {
                        throw new PiwikAPIException(result["message"][0].ToString());
                    }
                    else
                    {
                        Boolean resultStatus = false;
                        if (resultStatus is T && resultString.Equals("success"))
                        {
                            resultStatus = true;
                            return (T)(Object)resultStatus;
                        }

                        throw new PiwikAPIException(
                            "The server response does not match the expected return type. " +
                            "Please contact the developer with the following details : " +
                            "responseData = " + responseData + ", deserializedObject.getType() = " + deserializedObject.GetType()
                        );
                    }
                }
                else
                {
                    throw new PiwikAPIException(
                        "The server response has an unknown format. " +
                        "Please contact the developer with the following details : " +
                        "responseData = " + responseData + ", deserializedObject.getType() = " + deserializedObject.GetType()
                    );
                }
            }

            return (T)deserializedObject;
        }
    }
}
