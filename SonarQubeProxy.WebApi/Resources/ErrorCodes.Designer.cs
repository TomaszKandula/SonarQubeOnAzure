﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SonarQubeProxy.WebApi.Resources {
    using System;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class ErrorCodes {
        
        private static System.Resources.ResourceManager resourceMan;
        
        private static System.Globalization.CultureInfo resourceCulture;
        
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ErrorCodes() {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public static System.Resources.ResourceManager ResourceManager {
            get {
                if (object.Equals(null, resourceMan)) {
                    System.Resources.ResourceManager temp = new System.Resources.ResourceManager("SonarQubeProxy.Resources.ErrorCodes", typeof(ErrorCodes).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public static System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        public static string ERROR_UNEXPECTED {
            get {
                return ResourceManager.GetString("ERROR_UNEXPECTED", resourceCulture);
            }
        }
        
        public static string INVALID_ARGUMENT {
            get {
                return ResourceManager.GetString("INVALID_ARGUMENT", resourceCulture);
            }
        }
        
        public static string ARGUMENT_EMPTY_OR_NULL {
            get {
                return ResourceManager.GetString("ARGUMENT_EMPTY_OR_NULL", resourceCulture);
            }
        }
        
        public static string HTTP_REQUEST_FAILED {
            get {
                return ResourceManager.GetString("HTTP_REQUEST_FAILED", resourceCulture);
            }
        }
        
        public static string CANNOT_PARSE {
            get {
                return ResourceManager.GetString("CANNOT_PARSE", resourceCulture);
            }
        }
    }
}
