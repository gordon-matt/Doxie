// ----------------------------------------------------------------------------
// Based on Jolt.NET's implementation
// Original Code: https://jolt.codeplex.com
// RedGate's Fork on GitHub: https://github.com/red-gate/JoltNet-core

/*
License:  New BSD License
Copyright (c) 2007, Steve Guidi
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of Jolt.NET nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
// ----------------------------------------------------------------------------

using System.Xml.Linq;

namespace Doxie.Core.XmlComments
{
    /// <summary>
    /// Defines a contract for retrieving XML data from any
    /// XML doc comment data store.
    /// </summary>
    public interface IXmlDocCommentReadPolicy
    {
        /// <summary>
        /// Reads the XML indexed by the member of the given name from the
        /// underlying store.
        /// </summary>
        ///
        /// <param name="memberName">
        /// The name of the member for which the XML doc comments are read.
        /// </param>
        ///
        /// <returns>
        /// An <see cref="XElement"/> containing the requested XML doc comments.
        /// </returns>
        XElement ReadMember(string memberName);
    }
}