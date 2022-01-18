# Headless Web Container

Application to open web pages without any toolbars.

## 🚀 Build Status
[![Build Status](https://masch0212.visualstudio.com/MaSch/_apis/build/status/MaSch0212.headless-web-container?branchName=main)](https://masch0212.visualstudio.com/MaSch/_build/latest?definitionId=7&branchName=main)

## 🐱‍🏍 Getting Started

1. Install [Microsoft Visual C++ 2019 Redistributable](https://docs.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170#visual-studio-2015-2017-2019-and-2022). A minimum of VC++ 2019 is required, higher versions are backwards compatible.
2. Download and extract the ZIP file from the [latest release](https://github.com/MaSch0212/headless-web-container/releases/latest) to some folder 
3. Start the `HeadlessWebContainer.exe`.

You can then add profiles and save Shortcuts to these profiles. You can then also open a profile by calling the exe with the following arguments: `run -P <profile-name>`

If you do not want to create a profile, you can also just call the exe with the following arguments: `run -u <url> [-i <icon-path>] [-t <window-title>] [-T <theme-file>]`
