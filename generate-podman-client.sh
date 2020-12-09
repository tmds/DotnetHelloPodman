#!/bin/bash

dotnet tool install --global NSwag.ConsoleCore --version 13.9.4
nswag openapi2csclient  /Input:podman-swagger.yaml                  \
                        /Namespace:Podman                           \
                        /Output:PodmanClient.cs                     \
                        /ClassName:PodmanClient                     \
                        /GenerateOptionalParameters:true            \
                        /ArrayType:System.Collections.Generic.List  \
                        /ArrayInstanceType:System.Collections.Generic.List  \
                        /ArrayBaseType:System.Collections.Generic.List  \
                        /DictionaryType:System.Collections.Generic.Dictionary  \
                        /DictionaryInstanceType:System.Collections.Generic.Dictionary  \
                        /DictionaryBaseType:System.Collections.Generic.Dictionary
