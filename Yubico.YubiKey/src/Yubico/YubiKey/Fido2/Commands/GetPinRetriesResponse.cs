// Copyright 2022 Yubico AB
//
// Licensed under the Apache License, Version 2.0 (the "License").
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Yubico.Core.Iso7816;

namespace Yubico.YubiKey.Fido2.Commands
{
    /// <summary>
    /// This is the partner response class to the <see cref="GetPinRetriesCommand" /> command class.
    /// </summary>
    public class GetPinRetriesResponse : IYubiKeyResponseWithData<int>
    {
        private readonly ClientPinResponse _response;

        /// <summary>
        /// Constructs a new instance of the <see cref="GetPinRetriesResponse"/> class based on a response APDU provided
        /// by the YubiKey.
        /// </summary>
        /// <param name="responseApdu">
        /// A response APDU containing the CBOR response for the `getPINRetries` sub-command of the `authenticatorClientPIN`
        /// CTAP2 command.
        /// </param>
        public GetPinRetriesResponse(ResponseApdu responseApdu)
        {
            _response = new ClientPinResponse(responseApdu);
        }

        /// <summary>
        /// Returns the number of PIN retries remaining for this YubiKey's FIDO application.
        /// </summary>
        public int GetData()
        {
            int? pinRetries = _response.GetData().PinRetries;

            if (pinRetries is null)
            {
                throw new Ctap2DataException(); // TODO
            }

            return pinRetries.Value;
        }

        /// <inheritdoc />
        public ResponseStatus Status => _response.Status;

        /// <inheritdoc />
        public short StatusWord => _response.StatusWord;

        /// <inheritdoc />
        public string StatusMessage => _response.StatusMessage;
    }
}
