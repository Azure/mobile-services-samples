<!--
#
# Licensed to the Apache Software Foundation (ASF) under one
# or more contributor license agreements.  See the NOTICE file
# distributed with this work for additional information
# regarding copyright ownership.  The ASF licenses this file
# to you under the Apache License, Version 2.0 (the
# "License"); you may not use this file except in compliance
# with the License.  You may obtain a copy of the License at
# 
# http://www.apache.org/licenses/LICENSE-2.0
# 
# Unless required by applicable law or agreed to in writing,
# software distributed under the License is distributed on an
# "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
#  KIND, either express or implied.  See the License for the
# specific language governing permissions and limitations
# under the License.
#
-->
# Release Notes

### 0.2.1 (Sept 5, 2013)
* removed extraneous print statement
* [CB-4432] copyright notice change

### 0.2.3 (Sept 25, 2013)
* CB-4889 bumping&resetting version
* [windows8] commandProxy has moved
* [BlackBerry10] removed uneeded permission tags in plugin.xml
* CB-4889 renaming org.apache.cordova.core.device to org.apache.cordova.device
* Rename CHANGELOG.md -> RELEASENOTES.md
* updated to use commandProxy for ffos
* add firefoxos support
* [CB-4752] Incremented plugin version on dev branch. 

### 0.2.4 (Oct 28, 2013)
* CB-5128: added repo + issue tag in plugin.xml for device plugin
* CB-5085 device.cordova returning wrong value
* [CB-4915] Incremented plugin version on dev branch.

### 0.2.5 (Dec 4, 2013)
* CB-5316 Spell Cordova as a brand unless it's a command or script
* [ubuntu] use cordova/exec/proxy
* add ubuntu platform
* Modify Device.platform logic to use amazon-fireos as the platform for Amazon Devices
* 1. Added amazon-fireos platform. 2. Change to use cordova-amazon-fireos as the platform if user agent contains 'cordova-amazon-fireos'

### 0.2.6 (Jan 02, 2014)
* CB-5658 Add doc/index.md for Device plugin
* CB-5504 Moving Telephony Logic out of Device

### 0.2.7 (Jan 07, 2014)
* CB-5737 Fix exception on close caused by left over telephony code from CB-5504

### 0.2.8 (Feb 05, 2014)
* Tizen support added

### 0.2.9 (Apr 17, 2014)
* CB-5105: [Android, windows8, WP, BlackBerry10] Removed dead code for device.version
* CB-6422: [windows8] use cordova/exec/proxy
* CB-6460: Update license headers
* Add NOTICE file

### 0.2.10 (Jun 05, 2014)
* CB-6127 Spanish and French Translations added. Github close #12
* Changing 1.5 to 2.0
* added firefoxos version - conversion
* added firefoxos version
* CB-6800 Add license
* CB-6491 add CONTRIBUTING.md

### 0.2.11 (Aug 06, 2014)
* [FFOS] update DeviceProxy.js
* CB-6127 Updated translations for docs
* Use Windows system calls to get better info

### 0.2.12 (Sep 17, 2014)
* CB-7471 cordova-plugin-device documentation translation
* CB-7552 device.name docs have not been removed
* [fxos] Fix cordova version
* added status box and documentation to manual tests
* [fxos] Fix cordova version
* added status box and documentation to manual tests
* Added plugin support for the browser
* CB-7262 Adds support for universal windows apps.

### 0.2.13 (Dec 02, 2014)
* Changing `device.platform` to always report the platform as "browser".
* CB-5892 - Remove deprecated `window.Settings`
* CB-7700 cordova-plugin-device documentation translation: cordova-plugin-device
* CB-7571 Bump version of nested plugin to match parent plugin

### 0.3.0 (Feb 04, 2015)
* Added device.manufacturer property for Android, iOS, Blackberry, WP8
* Support for Windows Phone 8 ANID2 ANID is only supported up to Windows Phone 7.5
* CB-8351 Use a local copy of uniqueAppInstanceIdentifier rather than CordovaLib's version
* browser: Fixed a bug that caused an "cannot call method of undefined" error if the browser's user agent wasn't recognized
