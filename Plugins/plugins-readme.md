
All plugin projects must reference `Common.dll` like so:
```xml
  <ItemGroup>
	  <ProjectReference Include="..\..\Common\Common.csproj">
		  <Private>false</Private>
		  <ExcludeAssets>runtime</ExcludeAssets>
	  </ProjectReference>
  </ItemGroup>
```

And must set the following project settings:
```xml
  <PropertyGroup>
	...

	<EnableDynamicLoading>true</EnableDynamicLoading>

	...
  </PropertyGroup>

```
