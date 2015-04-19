﻿// -----------------------------------------------------------------------
//  <copyright file="ExceptionHandlingAttribute.cs" company="OSharp开源团队">
//      Copyright (c) 2014-2015 OSharp. All rights reserved.
//  </copyright>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2015-02-07 15:22</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http.Filters;

using OSharp.Utility.Logging;


namespace OSharp.Web.Http.Filters
{
    public class ExceptionHandlingAttribute : ExceptionFilterAttribute
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(ExceptionHandlingAttribute));

        static ExceptionHandlingAttribute()
        {
            Mappings = new Dictionary<Type, HttpStatusCode>
            {
                { typeof(ArgumentNullException), HttpStatusCode.BadRequest },
                { typeof(ArgumentException), HttpStatusCode.BadRequest }
            };
        }

        public static IDictionary<Type, HttpStatusCode> Mappings { get; private set; }

        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception == null)
            {
                return;
            }
            HttpRequestMessage request = actionExecutedContext.Request;
            Exception exception = actionExecutedContext.Exception;
            Logger.Error(exception.Message, exception);

            if (actionExecutedContext.Exception is HttpException)
            {
                HttpException httpException = (HttpException)exception;
                actionExecutedContext.Response =
                    request.CreateResponse((HttpStatusCode)httpException.GetHttpCode(), new Error { Message = exception.Message });
            }
            else if (Mappings.ContainsKey(exception.GetType()))
            {
                HttpStatusCode httpStatusCode = Mappings[exception.GetType()];
                actionExecutedContext.Response =
                    request.CreateResponse(httpStatusCode, new Error { Message = exception.Message });
            }
            else
            {
                actionExecutedContext.Response =
                    actionExecutedContext.Request.CreateResponse(HttpStatusCode.InternalServerError, new Error { Message = exception.Message });
            }
        }
    }


    public class Error
    {
        public string Name { get; set; }

        public string Message { get; set; }
    }
}