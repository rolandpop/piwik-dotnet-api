﻿// Copyright (c) Roland Pop All rights reserved.
// Licensed under the BSD 2-clause "Simplified" License. See License.txt in the project root for license information.

using System;

namespace Piwik.Analytics.Parameters
{
    class SimpleParameter : Parameter
    {
        private string value;

        public SimpleParameter(string name, string value) : base (name)
        {
            this.value = value;
        }

        public SimpleParameter(string name, bool value)
            : base(name)
        {
            this.value = value ? "1" : "0";
        }

        public SimpleParameter(string name, int value)
            : base(name)
        {
            this.value = value.ToString();
        }

        public override string serialize()
        {
            string parameter = String.Empty;

            if (!String.IsNullOrEmpty(this.value))
            {
                parameter = "&" + this.name + "=" + urlEncode(this.value);
            }

            return parameter;
        }
    }
}
