[![New Relic Experimental header](https://github.com/newrelic/opensource-website/raw/master/src/images/categories/Experimental.png)](https://opensource.newrelic.com/oss-category/#new-relic-experimental)

![GitHub forks](https://img.shields.io/github/forks/newrelic-experimental/newrelic-experimental-FIT-template?style=social)
![GitHub stars](https://img.shields.io/github/stars/newrelic-experimental/newrelic-experimental-FIT-template?style=social)
![GitHub watchers](https://img.shields.io/github/watchers/newrelic-experimental/newrelic-experimental-FIT-template?style=social)

![GitHub all releases](https://img.shields.io/github/downloads/newrelic-experimental/newrelic-experimental-FIT-template/total)
![GitHub release (latest by date)](https://img.shields.io/github/v/release/newrelic-experimental/newrelic-experimental-FIT-template)
![GitHub last commit](https://img.shields.io/github/last-commit/newrelic-experimental/newrelic-experimental-FIT-template)
![GitHub Release Date](https://img.shields.io/github/release-date/newrelic-experimental/newrelic-experimental-FIT-template)


![GitHub issues](https://img.shields.io/github/issues/newrelic-experimental/newrelic-experimental-FIT-template)
![GitHub issues closed](https://img.shields.io/github/issues-closed/newrelic-experimental/newrelic-experimental-FIT-template)
![GitHub pull requests](https://img.shields.io/github/issues-pr/newrelic-experimental/newrelic-experimental-FIT-template)
![GitHub pull requests closed](https://img.shields.io/github/issues-pr-closed/newrelic-experimental/newrelic-experimental-FIT-template)

# newrelic-dotnet-grpc [build badges go here when available]

This New Relic .Net agent instrumentation for [Grpc.Core](https://www.nuget.org/packages/Grpc.Core) module provides distributed tracing support for Grpc.Core rpc calls. 

## Installation

The binaries are builts for both .net core (netstandard2.0) and .net framework (net461). Use the appropriate build that targets your application plaform. For .net framework, change ***netcore*** to ***netframework*** in the destination paths below

1. Drop the extension ***dll*** file in the newrelic agent's Program Files "extensions" folder. 

```cmd
   copy Custom.Providers.Wrapper.Asp35.dll C:\Program Files\New Relic\.NET Agent\netcore\Extensions
```

2. Drop the extension ***xml*** file in the newrelic agent ProgramData "extensions" folder.

```cmd
   copy Custom.Providers.Wrapper.Asp35.xml C:\ProgramData\New Relic\.NET Agent\netcore\Extensions
```

***
**Note: The XML file must be dropped into ProgramData's extension folder whereas DLL file must be dropped into Program Files's extension folder**
***


## Usage

This package automatically adds newrelic distributed tracing headers to the Grpc client calls and extracts them on the Grpc server side. 

However this instrumentation cannot create headers if they are not initializated (null). So make sure the headers object is not null. 

For example, this rpc call does not initialize headers and hence they are null.

`var response = await client.SayHelloAsync(new HelloRequest { Name = "World" });`

So developers must instead change their call usage to initialize an non-null headers object instead.

`var response = await client.SayHelloAsync(new HelloRequest { Name = "World" }, new Metadata());`


## Support

New Relic has open-sourced this project. This project is provided AS-IS WITHOUT WARRANTY OR DEDICATED SUPPORT. Issues and contributions should be reported to the project here on GitHub.

We encourage you to bring your experiences and questions to the [Explorers Hub](https://discuss.newrelic.com) where our community members collaborate on solutions and new ideas.


## Contributing

We encourage your contributions to improve *newrelic-dotnet-grpc instrumentation*. Keep in mind when you submit your pull request, you'll need to sign the CLA via the click-through using CLA-Assistant. You only have to sign the CLA one time per project. If you have any questions, or to execute our corporate CLA, required if your contribution is on behalf of a company, please drop us an email at opensource@newrelic.com.

**A note about vulnerabilities**

As noted in our [security policy](../../security/policy), New Relic is committed to the privacy and security of our customers and their data. We believe that providing coordinated disclosure by security researchers and engaging with the security community are important means to achieve our security goals.

If you believe you have found a security vulnerability in this project or any of New Relic's products or websites, we welcome and greatly appreciate you reporting it to New Relic through [HackerOne](https://hackerone.com/newrelic).

## License

*newrelic-dotnet-grpc instrumentation* is licensed under the [Apache 2.0](http://apache.org/licenses/LICENSE-2.0.txt) License.
