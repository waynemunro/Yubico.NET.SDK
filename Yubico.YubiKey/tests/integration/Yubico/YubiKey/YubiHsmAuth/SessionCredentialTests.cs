﻿// Copyright 2022 Yubico AB
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

using System;
using System.Collections.Generic;
using System.Security;
using Xunit;
using Yubico.YubiKey.TestUtilities;

namespace Yubico.YubiKey.YubiHsmAuth
{
    public class SessionCredentialTests
    {
        [Fact]
        public void AddCredential_DefaultTestCred_AppContainsOneCred()
        {
            // Preconditions
            IYubiKeyDevice testDevice = YhaTestUtilities.GetCleanDevice();

            IReadOnlyList<CredentialRetryPair> credentialList;

            // Test
            using (var yubiHsmAuthSession = new YubiHsmAuthSession(testDevice))
            {
                yubiHsmAuthSession.AddCredential(YhaTestUtilities.DefaultMgmtKey, YhaTestUtilities.DefaultAes128Cred);

                credentialList = yubiHsmAuthSession.ListCredentials();
            }

            // Postconditions
            Assert.True(credentialList.Count == 1);
        }

        [Fact]
        public void TryDeleteCredentialKeyCollector_CorrectMgmtKey_AppContainsZeroCreds()
        {
            // Preconditions
            IYubiKeyDevice testDevice = YhaTestUtilities.GetCleanDevice();
            YhaTestUtilities.AddDefaultAes128Credential(testDevice);

            IReadOnlyList<CredentialRetryPair> credentialList;

            using (var yubiHsmAuthSession = new YubiHsmAuthSession(testDevice))
            {
                yubiHsmAuthSession.KeyCollector = SimpleKeyCollector.DefaultValueCollectorDelegate;

                // Test
                _ = yubiHsmAuthSession.TryDeleteCredential(YhaTestUtilities.DefaultCredLabel);

                credentialList = yubiHsmAuthSession.ListCredentials();
            }

            // Postconditions
            Assert.True(credentialList.Count == 0);
        }

        [Fact]
        public void TryDeleteCredentialKeyCollector_MgmtKeyRetry_AppContainsZeroCreds()
        {
            // Preconditions
            IYubiKeyDevice testDevice = YhaTestUtilities.GetCleanDevice();
            YhaTestUtilities.AddDefaultAes128Credential(testDevice);

            IReadOnlyList<CredentialRetryPair> credentialList;

            using (var yubiHsmAuthSession = new YubiHsmAuthSession(testDevice))
            {
                SimpleKeyCollector keyCollector = new SimpleKeyCollector
                {
                    // Start with the incorrect management key, forcing a retry
                    UseDefaultValue = false
                };
                yubiHsmAuthSession.KeyCollector = keyCollector.FlipFlopCollectorDelegate;

                // Test
                _ = yubiHsmAuthSession.TryDeleteCredential(YhaTestUtilities.DefaultCredLabel);

                credentialList = yubiHsmAuthSession.ListCredentials();
            }

            // Postconditions
            Assert.True(credentialList.Count == 0);
        }

        [Fact]
        public void TryDeleteCredentialKeyCollector_KeyCollectorReturnsFalse_ReturnsFalse()
        {
            // Preconditions
            IYubiKeyDevice testDevice = YhaTestUtilities.GetCleanDevice();
            YhaTestUtilities.AddDefaultAes128Credential(testDevice);

            bool deleteCredSuccess = false;

            using (var yubiHsmAuthSession = new YubiHsmAuthSession(testDevice))
            {
                yubiHsmAuthSession.KeyCollector = SimpleKeyCollector.ReturnsFalseCollectorDelegate;

                // Test
                deleteCredSuccess = yubiHsmAuthSession.TryDeleteCredential(YhaTestUtilities.DefaultCredLabel);
            }

            // Postconditions
            Assert.False(deleteCredSuccess);
        }

        [Fact]
        public void TryDeleteCredentialKeyCollector_WrongMgmtKey_ThrowsSecurityException()
        {
            // Preconditions
            IYubiKeyDevice testDevice = YhaTestUtilities.GetCleanDevice();
            YhaTestUtilities.AddDefaultAes128Credential(testDevice);

            using (var yubiHsmAuthSession = new YubiHsmAuthSession(testDevice))
            {
                // Use incorrect management key, exhausting retries
                yubiHsmAuthSession.KeyCollector = SimpleKeyCollector.AlternateValueCollectorDelegate;

                // Test & postcondition
                void tryDeleteCred() => yubiHsmAuthSession.TryDeleteCredential(YhaTestUtilities.DefaultCredLabel);

                _ = Assert.Throws<SecurityException>(tryDeleteCred);
            }
        }

        [Fact]
        public void TryDeleteCredentialKeyCollector_CredNotFound_ThrowsInvalidOpEx()
        {
            // Preconditions
            IYubiKeyDevice testDevice = YhaTestUtilities.GetCleanDevice();
            YhaTestUtilities.AddDefaultAes128Credential(testDevice);

            using (var yubiHsmAuthSession = new YubiHsmAuthSession(testDevice))
            {
                // Use incorrect management key, exhausting retries
                yubiHsmAuthSession.KeyCollector = SimpleKeyCollector.DefaultValueCollectorDelegate;

                // Test & postcondition
                void tryDeleteCred() => yubiHsmAuthSession.TryDeleteCredential(YhaTestUtilities.AlternateCredLabel);

                _ = Assert.Throws<InvalidOperationException>(tryDeleteCred);
            }
        }

        [Fact]
        public void TryDeleteCredentialKeyCollector_NoKeyCollector_ThrowsInvalidOpEx()
        {
            // Preconditions
            IYubiKeyDevice testDevice = YhaTestUtilities.GetCleanDevice();
            YhaTestUtilities.AddDefaultAes128Credential(testDevice);

            using (var yubiHsmAuthSession = new YubiHsmAuthSession(testDevice))
            {
                // Leave yubiHsmAuthSession.KeyCollector set to its default value of `null`

                // Test & postcondition
                void tryDeleteCred() => yubiHsmAuthSession.TryDeleteCredential(YhaTestUtilities.DefaultCredLabel);

                _ = Assert.Throws<InvalidOperationException>(tryDeleteCred);
            }
        }
    }
}
