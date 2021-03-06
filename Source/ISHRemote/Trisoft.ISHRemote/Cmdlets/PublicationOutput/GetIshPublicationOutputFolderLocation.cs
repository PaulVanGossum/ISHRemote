/*
* Copyright (c) 2014 All Rights Reserved by the SDL Group.
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* 
*     http://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Xml;
using System.Management.Automation;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.HelperClasses;

namespace Trisoft.ISHRemote.Cmdlets.PublicationOutput
{
    /// <summary>
    /// <para type="synopsis">The Get-IshPublicationOutputFolderLocation cmdlet returns the repository location of the publication output in the form of a separated string e.g. "General\Folder1\Folder2"</para>
    /// <para type="description">The Get-IshPublicationOutputFolderLocation cmdlet returns the repository location of the publication output in the form of a separated string e.g. "General\Folder1\Folder2"</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $folderLocation = Get-IshPublicationOutputFolderLocation -LogicalId "GUID-412E3A98-9AA8-484E-A1AA-3DE3B58947BD"
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Retrieve location of the PublicationOutput</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "IshPublicationOutputFolderLocation", SupportsShouldProcess = false)]
    [OutputType(typeof(string))]
    public sealed class GetIshPublicationOutputFolderLocation :PublicationOutputCmdlet
    {

        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The LogicalId of the PublicationOutput object</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string LogicalId { get; set; }

        /// <summary>
        /// <para type="description">Publication output for which to retrieve the content data. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectGroup")]
        public IshObject[] IshObject { get; set; }

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            base.BeginProcessing();
        }

        /// <summary>
        /// Process the Get-IshPublicationOutputFolderLocation commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        protected override void ProcessRecord()
        {
            try
            {
                WriteDebug("Validating");
                if (IshObject != null)
                {
                    int current = 0;
                    foreach (IshObject ishObject in IshObject)
                    {
                        WriteDebug($"lngRef[{ishObject.ObjectRef[Enumerations.ReferenceType.Lng]}] {++current}/{IshObject.Length}");
                        // 1. Validating the input
                        // Use provided pipeline's IshObject or use provided LogicalId
                        string logicalId = ishObject.IshRef;

                        // 2. Call PublicationOutput25.FolderLocation using provided LogicalId
                        WriteDebug($"Retrieving PublicationOutput FolderLocation LogicalId[{logicalId}]");

                        var response = IshSession.PublicationOutput25.FolderLocation(
                            new PublicationOutput25ServiceReference.FolderLocationRequest(logicalId));

                        // Using basefolder name and folderPaths array make a full folder path
                        var folderPaths = new List<string>() { BaseFolderEnumToLabel(IshSession,
                        EnumConverter.ToBaseFolder<Folder25ServiceReference.BaseFolder>(response.baseFolder.ToString())) };
                        folderPaths.AddRange(response.folderPath);

                        // 3. Write full folder path to the pipeline
                        WriteVerbose("returned folderlocation count[1]");
                        WriteObject(IshSession.FolderPathSeparator + string.Join(IshSession.FolderPathSeparator, folderPaths));
                    }
                }
                // LogicalId is provided
                else
                {
                    var response = IshSession.PublicationOutput25.FolderLocation(
                            new PublicationOutput25ServiceReference.FolderLocationRequest(LogicalId));
                    // Using basefolder name and folderPaths array make a full folder path
                    var folderPaths = new List<string>() { BaseFolderEnumToLabel(IshSession,
                        EnumConverter.ToBaseFolder<Folder25ServiceReference.BaseFolder>(response.baseFolder.ToString())) };
                    folderPaths.AddRange(response.folderPath);
                    WriteObject(IshSession.FolderPathSeparator + string.Join(IshSession.FolderPathSeparator, folderPaths));
                }
            }
            catch (TrisoftAutomationException trisoftAutomationException)
            {
                ThrowTerminatingError(new ErrorRecord(trisoftAutomationException, base.GetType().Name, ErrorCategory.InvalidOperation, null));
            }
            catch (Exception exception)
            {
                ThrowTerminatingError(new ErrorRecord(exception, base.GetType().Name, ErrorCategory.NotSpecified, null));
            }
        }
    }
}
