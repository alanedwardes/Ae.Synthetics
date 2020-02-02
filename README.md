# Ae.Synthetics ![https://www.nuget.org/packages/Ae.Synthetics](https://img.shields.io/nuget/v/Ae.Synthetics.svg)
A bootstrapper for synthetic tests that are run on a schedule with metrics and alerting.

## Summary
* Run multiple testing `Task` (defined as C# `ISyntheticTest`) in parallel and alert on failure
* Designed to be deployed to AWS Lambda and run on a CloudWatch schedule
* Send alerts to Amazon SES or custom back-end via a generic `ISyntheticsAlerter` interface
* Built-in logging for tests to make diagnosing failure easy (log messages are passed to alerters)

## Usage
This package is designed for use with a scheduled AWS Lambda function, but can also be run as a console app.

### Defining Tests
Tests are defined using the `ISyntheticTest` interface. If the test runs without throwing an exception it is considered successful, and if it throws an exception it is considered failed.

When a test fails, the runner will invoke the `ISyntheticsAlerter` interface - an implementation for Amazon SES is available to alert of failures via email.
```csharp
internal sealed class MyTest : ISyntheticTest
{
    public async Task Run(ILogger logger, CancellationToken token)
    {
        using (var client = new HttpClient())
        {
            logger.LogInformation($"Calling my endpoint");
            var response = await client.GetAsync("https://www.example.com/");
            response.EnsureSuccessStatusCode();
        }
    }
}
```

### Console App Runner
Once one or more tests are defined, a console app can be defined to run them locally (and alert via SES upon test failure):

```csharp
public static class Program
{
    public static void Main()
    {
        var provider = new ServiceCollection()
            // Register all synthetic tests against the ISyntheticTest interface
            .AddSingleton<ISyntheticTest, MyTest>()
            // Configure the runner - ensure tests are given 3 seconds to complete
            .AddSyntheticsRunner(new SyntheticsRunnerConfig
            {
                // Configure the runner - ensure tests are given 3 seconds to complete
                SyntheticTestTimeout = TimeSpan.FromSeconds(3)
            })
            .AddSyntheticsSesAlerting(new SesSyntheticsAlerterConfig
            {
                // Add alerting of failures via Amazon SES
                Recipients = new[] { "test@example.com" },
                Sender = "synthetics@example.com"
            })
            .BuildServiceProvider();

        provider.GetRequiredService<ISyntheticsRunner>()
                .RunSyntheticTestsForever(TimeSpan.FromMinutes(5), CancellationToken.None)
                .GetAwaiter()
                .GetResult();
    }
}
```

### Lambda Function Runner
To deploy the tests to AWS, define a C# Lambda function in a similar way to the console app:

```csharp
public class Function
{
    public Stream FunctionHandlerAsync(Stream input, ILambdaContext context)
    {
        var provider = new ServiceCollection()
            // Register all synthetic tests against the ISyntheticTest interface
            .AddSingleton<ISyntheticTest, MyTest>()
            .AddSyntheticsRunner(new SyntheticsRunnerConfig
            {
                // Configure the runner - ensure tests are given 3 seconds to complete
                SyntheticTestTimeout = TimeSpan.FromSeconds(3)
            })
            .AddSyntheticsSesAlerting(new SesSyntheticsAlerterConfig
            {
                // Add alerting of failures via Amazon SES
                Recipients = new[] { "test@example.com" },
                Sender = "synthetics@example.com"
            })
            .BuildServiceProvider();

        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        provider.GetRequiredService<ISyntheticsRunner>()
                .RunSyntheticTests(tokenSource.Token)
                .GetAwaiter()
                .GetResult();

        return Stream.Null;
    }
}
```

See the following example CloudFormation template to deploy the Lambda function on a 5 minute CloudWatch schedule:
```json
{
    "AWSTemplateFormatVersion": "2010-09-09",
    "Resources": {
        "SyntheticsFunction": {
            "Properties": {
                "Code": {
                    "S3Bucket": "",
                    "S3Key": ""
                },
                "Handler": "MyNamespace::MyNamespace.Function::FunctionHandlerAsync",
                "MemorySize": 128,
                "Role": {
                    "Fn::Sub": "${SyntheticsFunctionRole.Arn}"
                },
                "Runtime": "dotnetcore2.1",
                "Timeout": 30
            },
            "Type": "AWS::Lambda::Function"
        },
        "SyntheticsFunctionRole": {
            "Properties": {
                "AssumeRolePolicyDocument": {
                    "Statement": [
                        {
                            "Action": [
                                "sts:AssumeRole"
                            ],
                            "Effect": "Allow",
                            "Principal": {
                                "Service": [
                                    "lambda.amazonaws.com"
                                ]
                            }
                        }
                    ],
                    "Version": "2012-10-17"
                },
                "ManagedPolicyArns": [
                    "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole",
                    "arn:aws:iam::aws:policy/AWSLambdaFullAccess"
                ],
                "Policies": [
                    {
                        "PolicyName": "root",
                        "PolicyDocument": {
                            "Version": "2012-10-17",
                            "Statement": [
                                {
                                    "Effect": "Allow",
                                    "Action": [
                                        "ses:SendEmail"
                                    ],
                                    "Resource": "*"
                                }
                            ]
                        }
                    }
                ]
            },
            "Type": "AWS::IAM::Role"
        },
        "SyntheticsRule": {
            "Type": "AWS::Events::Rule",
            "Properties": {
                "ScheduleExpression": "rate(5 minutes)",
                "Targets": [
                    {
                        "Id": "SyntheticsScheduler",
                        "Arn": {
                            "Fn::GetAtt": [
                                "SyntheticsFunction",
                                "Arn"
                            ]
                        }
                    }
                ]
            }
        },
        "InvokeLambdaPermission": {
            "Type": "AWS::Lambda::Permission",
            "Properties": {
                "FunctionName": {
                    "Fn::GetAtt": [
                        "SyntheticsFunction",
                        "Arn"
                    ]
                },
                "Action": "lambda:InvokeFunction",
                "Principal": "events.amazonaws.com",
                "SourceArn": {
                    "Fn::GetAtt": [
                        "SyntheticsRule",
                        "Arn"
                    ]
                }
            }
        }
    }
}
```