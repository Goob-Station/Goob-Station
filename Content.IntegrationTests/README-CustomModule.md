If you want tests to pass while testing against your custom module you have to:
* Add Client and Shared assemblies to PoolManager.cs:GenerateClient()
* Add Shared and Server assemblies to PoolManager.cs:Startup()
* Add Client and Shared assemblies to SandboxTest.cs

It is abysmal dogshit, but this is this, and that is that.
