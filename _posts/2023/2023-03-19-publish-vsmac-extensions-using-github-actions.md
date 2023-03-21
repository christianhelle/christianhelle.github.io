---
layout: post
title: Publish Visual Studio for Mac extensions using Github Actions
date: '2023-03-19'
author: Christian Helle
tags: 
- Visual Studio for Mac
redirect_from:
- /2023/03/publish-vsmac-extensions-using-github-actions/
- /2023/03/publish-vsmac-extensions-using-github-actions
- /2023/publish-vsmac-extensions-using-github-actions/
- /2023/publish-vsmac-extensions-using-github-actions
- /publish-vsmac-extensions-using-github-actions/
- /publish-vsmac-extensions-using-github-actions
---

In my previous article on [Build Visual Studio for Mac Extensions using Github Actions](/2023/03/build-vsmac-extensions-using-github-actions.html), I went through how to build a [Visual Studio for Mac](https://visualstudio.microsoft.com/vs/mac?WT.mc_id=DT-MVP-5004822) extension using [Github Actions](https://docs.github.com/en/actions/quickstart?WT.mc_id=DT-MVP-5004822). In this article, I would like to go into further detail of the same theme but now focusing on how to publish a Visual Studio for Mac extension to a private extension repository hosted in Github, using [Github Actions](https://docs.github.com/en/actions/quickstart?WT.mc_id=DT-MVP-5004822)

## Create extensions repository

First thing we need to is to [Create a new Github repository](https://github.com/new?WT.mc_id=DT-MVP-5004822) to publish our builds to. You can do this by either clicking on the **New** button from your Github Profile page under **Repositories** or by clicking [Here](https://github.com/new?WT.mc_id=DT-MVP-5004822)

![](/assets/images/new-vsmac-extension-repo-from-profile.png)

![](/assets/images/new-vsmac-extension-repo.png)

or by using the [Github CLI](https://cli.github.com/?WT.mc_id=DT-MVP-5004822)

```bash
$ gh repo create my-vsmac-extension-repo --public
```

We need this to be a **Public** repository as we will need to access the raw contents of our repository and add it to Visual Studio for Mac

Now that we have an initial repo, we need to create a branch to commit to. You can't push commits to an empty repo

```bash
echo "# delete-me" >> README.md
git init
git add README.md
git commit -m "first commit"
git branch -M main
git remote add origin git@github.com:christianhelle/my-vsmac-extension-repo.git
git push -u origin main
```

Now that we have a repository, we need to [Create a personal access token](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/creating-a-personal-access-token?WT.mc_id=DT-MVP-5004822)

![](/assets/images/github-developer-settings-pat.png)

and give it access to push commits to our new repository by giving the PAT the `public_repo` scope

![](/assets/images/github-pat-scope-public-repo.png)

Copy the PAT to the clipboard as we will use this PAT

![](/assets/images/github-pat-copy.png)

Store the PAT as a Github Actions Secret in the repository that builds the Visual Studio for Mac extension. From the Settings page of that repo, unfold **Secrets and variables**, select **Actions**, then click on **New Repository Secret**.

![](/assets/images/github-actions-secret-new.png)

Give the **Secret** a name, in this case `ACTIONS_GITHUB_TOKEN` and paste the PAT that we just created

![](/assets/images/github-actions-secret-new-save.png)

## Build and Publish .mpack file

The next thing we need to is to update our workflow to publish the `.mpack` file to the new repo we created. We do this by adding a new job to our existing workflow:

{% raw %}
```yml
  deploy:

    runs-on: ubuntu-latest
    timeout-minutes: 10
    needs: build
    if: github.ref == 'refs/heads/main'

    steps:
    - uses: actions/checkout@v3
      with:
        repository: christianhelle/my-vsmac-extension-repo
        ref: 'main'
        token:  ${{ secrets.ACTIONS_GITHUB_TOKEN }}

    - uses: actions/download-artifact@v3
      with:
        path: artifacts

    - name: Remove version number from filename
      run: |
        mv artifacts/Extension/Sample-${{ env.VERSION }}.mpack Sample.mpack
        rm -rf artifacts
    
    - name: Git Commit Build Artifacts      
      run: |
        git config --global user.name "Continuous Integration"
        git config --global user.email "username@users.noreply.github.com"
        git add Sample.mpack
        git commit -m "Update .mpack file to version ${{ env.VERSION }}"
        git push
```
{% endraw %}     

This new job will checkout our new Visual Studio for Mac extension repository, which in this example I called `my-vsmac-extension-repo`. We set the `token` parameter because we will be committing back to this repository. Then it downloads the build artifacts from the previous `build` job to a folder called `artifacts`. You can only provide one version of your extension at a time in a Visual Studio for Mac extension repository so we need to strip the version number out of the filename. Lastly, add and commit the `.mpack` file to the extensions repository

Here's the full contents of our workflow:

{% raw %}
```yml
name: Build

on:
  workflow_dispatch:
  push:

env:
  VERSION: 1.0.${{ github.run_number }}

jobs:

  build:

    runs-on: macos-latest
    timeout-minutes: 10

    steps:
    - uses: actions/checkout@v3

    - name: Update Extension Version Info
      run: |
        sed -i -e 's/1.0/${{ env.VERSION }}/g' ./AddinInfo.cs
        cat ./AddinInfo.cs
      working-directory: src

    - name: Restore
      run: dotnet restore
      working-directory: src

    - name: Build
      run: /Applications/Visual\ Studio.app/Contents/MacOS/vstool build --configuration:Release $PWD/Sample.csproj
      working-directory: src

    - name: Pack
      run: /Applications/Visual\ Studio.app/Contents/MacOS/vstool setup pack $PWD/src/bin/Release/net7.0/Sample.dll -d:$PWD

    - name: Archive binaries
      run: zip -r Binaries.zip src/bin/Release/net7.0/

    - name: Publish binaries
      uses: actions/upload-artifact@v2
      with:
        name: Binaries
        path: Binaries.zip

    - name: Rename build output
      run: mv *.mpack Sample-${{ env.VERSION }}.mpack

    - name: Publish artifacts
      uses: actions/upload-artifact@v2
      with:
        name: Extension
        path: Sample-${{ env.VERSION }}.mpack

  deploy:

    runs-on: ubuntu-latest
    timeout-minutes: 10
    needs: build
    if: github.ref == 'refs/heads/main'

    steps:
    - uses: actions/checkout@v3
      with:
        repository: christianhelle/my-vsmac-extension-repo
        ref: 'main'
        token:  ${{ secrets.ACTIONS_GITHUB_TOKEN }}

    - uses: actions/download-artifact@v3
      with:
        path: artifacts

    - name: Remove version number from filename
      run: |
        mv artifacts/Extension/Sample-${{ env.VERSION }}.mpack Sample.mpack
        rm -rf artifacts
    
    - name: Git Commit Build Artifacts      
      run: |
        git config --global user.name "Continuous Integration"
        git config --global user.email "username@users.noreply.github.com"
        git add Sample.mpack
        git commit -m "Update .mpack file to version ${{ env.VERSION }}"
        git push
```
{% endraw %}

A successful run of this build should look like something like this:

![](/assets/images/github-actions-build-deploy.png)

Take a look at the extensions repo to make sure that the `.mpack` file got pushed correctly

![](/assets/images/github-extensions-repo-with-mpack.png)

## Build the .mrep files

Now that we have .mpack files getting pushed into the extensions repository, we can setup a Github workflow that creates the Visual Studio for Mac `.mrep` files upon every push. The `.mrep` file is an XML file that contains meta data about the `.mpack` files in the extensions repository

Before we can grant Github Actions **Read and Write permissions** to its own repository. To do this, in the extensions repository **Settings**, unfold **Actions** and select **General**, enable **Read and Write permissions** under **Workflow Permissions**

![](/assets/images/github-actions-workflow-permissions.png)

Now we create a workflow:

```yml
{% raw %}
name: Build

on:
  workflow_dispatch:
  push:
    paths-ignore:
      - '**/*'
      - '!**/*.mpack'
      - '!.github/workflows/build.yml'

jobs:
  build:

    runs-on: macos-latest
    timeout-minutes: 10

    env: 
      CI_COMMIT_MESSAGE: Continuous Integration Build Artifacts
      CI_COMMIT_AUTHOR: Continuous Integration

    steps:
    - uses: actions/checkout@v3

    - name: stable - vstool setup rep-build
      run: /Applications/Visual\ Studio.app/Contents/MacOS/vstool setup rep-build $PWD

    - name: Publish Stable Repo
      uses: actions/upload-artifact@v2
      with:
        name: Stable
        path: |
          *.mrep
          index.html
    
    - name: Git Commit Build Artifacts
      run: |
        git config --global user.name "${{ env.CI_COMMIT_AUTHOR }}"
        git config --global user.email "username@users.noreply.github.com"
        git add .
        git commit -m "${{ env.CI_COMMIT_MESSAGE }}"
        git push
{% endraw %}
```

We can either manually run the workflow above, or we can just trigger it by running the build workflow on the Visual Studio for Mac extension sample.

Once the workflow above has ran, the extensions repository should look something like this:

![](/assets/images/github-extensions-repo-with-mrep.png)

Thw workflow above ran `vstool setup rep-build $PWD` and committed the output files to its own repo. The command `vstool setup rep-build $PWD` produces 3 files, `index.html`, `main.mrep`, and `root.mrep`

The `root.mrep` file describes the files in th repository and looks something like this:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Repository xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <Addin>
    <Url>Sample.mpack</Url>
    <Addin>
      <Id>Sample</Id>
      <Namespace>Sample</Namespace>
      <Name>My First Extension</Name>
      <Version>0.1.17</Version>
      <BaseVersion />
      <Author>Christian Resma Helle</Author>
      <Copyright />
      <Url />
      <Description>My first Visual Studio for Mac extension</Description>
      <Category>IDE extensions</Category>
      <Dependencies />
      <OptionalDependencies />
      <Properties>
        <Property name="DownloadSize">3188</Property>
      </Properties>
    </Addin>
  </Addin>
</Repository>
```

The `main.mrep` files points to the `root.mrep` file. You will be adding the direct link to the `main.mrep` file from Visual Studio for Mac

```xml
<?xml version="1.0" encoding="utf-8"?>
<Repository xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <Repository>
    <Url>root.mrep</Url>
    <LastModified>2023-03-20T15:31:42.3222075+00:00</LastModified>
  </Repository>
</Repository>
```

And lastly, `index.html` is a naive attempt to create a HTML page describing the available extensions

```html
<html><body>
<h1>Add-in Repository</h1>
<p>This is a list of add-ins available in this repository.</p>
<table border=1><thead><tr><th>Add-in</th><th>Version</th><th>Description</th></tr></thead>
<tr><td>My First Extension</td><td>0.1.17</td><td>My first Visual Studio for Mac extension</td></tr>
</table>
</body></html>
```

## Add extension repository to Visual Studio

Now all we need is to add a new custom extension repository to Visual Studio for Mac so we can download the extensions all from the IDE, and also see when there are updates coming in. To do this, we need to get the URL to the **Raw** `main.mrep` file. We do this by opening `main.mrep` from Github and getting the URL from the **Raw** link

![](/assets/images/github-extensions-repo-main-raw.png)

This should be something like `https://raw.githubusercontent.com/christianhelle/my-vsmac-extension-repo/main/main.mrep`. We need to add this as an **Extension Source** to Visual Studio for Mac. This is done from **Preferences**, then scroll down on the side menu to **Extensions**, select **Sources**, then click on **Add**, then paste the **Raw** link to `main.mrep`

![](/assets/images/vsmac-extension-sources.png)

![](/assets/images/vsmac-extension-sources-add.png)

After this we should be able to see our extension from the Visual Studio for Mac Extensions screen

![](/assets/images/vsmac-extensions-custom-source.png)

I hope you found this useful and get inspired to start building extensions of your own. If you're interested in the full source code then you can checkout the [Example VSMac extension project](https://github.com/christianhelle/extending-vsmac-sample) and the [Visual Studio for Mac extension repository](https://github.com/christianhelle/my-vsmac-extension-repo)