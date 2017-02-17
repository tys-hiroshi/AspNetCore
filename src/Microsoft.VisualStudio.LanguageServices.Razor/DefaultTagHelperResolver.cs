﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Evolution;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Razor;

namespace Microsoft.VisualStudio.LanguageServices.Razor
{
    [Export(typeof(ITagHelperResolver))]
    internal class DefaultTagHelperResolver : ITagHelperResolver
    {
        [Import]
        public VisualStudioWorkspace Workspace { get; set; }

        public async Task<TagHelperResolutionResult> GetTagHelpersAsync(Project project)
        {
            try
            {
                var client = await RazorLanguageServiceClientFactory.CreateAsync(Workspace, CancellationToken.None);
                if (client == null)
                {
                    // The OOP host is turned off, so let's do this in process.
                    var resolver = new CodeAnalysis.Razor.DefaultTagHelperResolver(designTime: true);
                    var result =  await resolver.GetTagHelpersAsync(project, CancellationToken.None).ConfigureAwait(false);
                    return result;
                }

                using (var session = await client.CreateSessionAsync(project.Solution))
                {
                    var result = await session.InvokeAsync<TagHelperResolutionResult>("GetTagHelpersAsync", new object[] { project.Id.Id, "Foo", }).ConfigureAwait(false);
                    return result;
                }
            }
            catch (Exception exception)
            {
                throw new RazorLanguageServiceException(
                    typeof(DefaultTagHelperResolver).FullName,
                    nameof(GetTagHelpersAsync),
                    exception);
            }
        }
    }
}
