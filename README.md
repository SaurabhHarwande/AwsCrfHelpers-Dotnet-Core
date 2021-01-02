# AwsCrfHelpers

Library was developed to assist with the creation of Aws Lambda Functions for Custom Resources Under CloudFormation.
# Installation
To use add the [Nuget Package](https://www.nuget.org/packages/AwsCrfHelper/) to your project
``` sh
dotnet add package AwsCrfHelper
```
# Usage
There is no documentation around this right now. I have a sample code that can be used as a reference.

Aws Lambda Function:
``` C#
using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.ElasticLoadBalancingV2;
using Amazon.ElasticLoadBalancingV2.Model;
using Amazon.Lambda.Core;
using CrfHelpers;
using CrfHelpers.Models;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace aws_listenser_rule_priority_generator
{
    public class Function
    {
        public async Task FunctionHandler(CrfRequestBody<CustomResourceRequestProperties> crfRequestBody, ILambdaContext context)
        {
            var crfRequestProcessor = new CrfRequestProcessor<CustomResourceRequestProperties, object>();
            crfRequestProcessor.Create += async (requestProperties, oldRequestProperties) =>
            {
                AmazonElasticLoadBalancingV2Client elbV2Client = new AmazonElasticLoadBalancingV2Client(RegionEndpoint.EUWest1);
                var describeRulesResponse = await elbV2Client.DescribeRulesAsync(new DescribeRulesRequest  {
                    ListenerArn = requestProperties.ListenerArn
                });
                var priority = 0;
                var random = new Random();
                do {
                    priority = random.Next(1, 50000);
                }
                while(describeRulesResponse.Rules.Exists(r => r.Priority == priority.ToString()));
                var responseData = new {
                    Priority = priority
                };
                return responseData;
            };
            crfRequestProcessor.Delete += async (requestProperties, oldRequestProperties) =>
            {
                return await Task.Run(() =>
                {
                    return new
                    {
                        Dummy = "Dummy"
                    };
                });
            };
            
            await crfRequestProcessor.ProcessAsync(crfRequestBody);
        }
    }
}
```
Model Class
``` C#
using CrfHelpers.Models.Interfaces;

namespace aws_listenser_rule_priority_generator {
    public class CustomResourceRequestProperties : ICrfRequestProperties
    {
        string ICrfRequestProperties.ServiceToken { get; set; }
        public string ListenerArn { get; set; }
    }
}
```
# Contributing
If you would like to request or report anything, please raise issues/pull request for the same.