# 波西亚时光 内置修改器


从[https://github.com/HearthSim/UnityHook](https://github.com/HearthSim/UnityHook)而来。

## 功能

 * [x]  添加物品
 * [x]  成就解锁
 * [x]  更新物价指数
 * [x]  忠诚不减(被动)
 * [x]  约会/玩耍心情满(被动)
 * [x]  约会不会被嫉妒(被动)
 * [x]  玫瑰花=100好感=1颗心(被动)

## SQL
#### 查询物品数据
```sql
SELECT Translation_hint.Chinese, Props_total_table.* FROM "Props_total_table" JOIN "Translation_hint" ON Props_total_table.Props_Name = Translation_hint.ID
WHERE Translation_hint.Chinese LIKE "冬娃%"
```
#### 查询古物
```sql
SELECT Translation_hint.Chinese,ExhibitData.*, Props_total_table.* FROM "Props_total_table" JOIN "Translation_hint", ExhibitData ON Props_total_table.Props_Name = Translation_hint.ID and ExhibitData.itemId = Props_total_table.Props_Id
```
#### 查询成就
```sql
SELECT 
(SELECT Chinese FROM Translation_hint WHERE Translation_hint.ID=name)as nameCN,
(SELECT Chinese FROM Translation_hint WHERE Translation_hint.ID=description)as descriptionCN,
* FROM "Achievement" 
```

## Hooker

**Hooker** is the project that actually injects code into original game assemblies/libraries (= .dll files).
It can be used to 'hook' and 'restore' assemblies. Run `hooker.exe help` for information about the options to pass.
To hook game-assemblies you need to tell it the location of the game folder and the path to a compiled **HookRegistry** binary.

## HookRegistry

**HookRegistry** is the project that contains code to be executed when a hooked method/function has been called
while the game is running. The project compiles to 1 binary file that must be passed to **Hooker**.
Currently implemented hooks are following:

> **Hooker** will attempt to copy all referenced (by **HookRegistry**) library files to the library folder of the game. Make sure to validate all necessary library files are copied by inspecting the **Hooker** log output.

## Hooks file
The file which declares all methods, located inside the game libraries, to be hooked. See `/Hooker/example_hooks` for more information about it's syntax. The example_hooks file is used in the next section's example.

> **NOTE:** The hooker will always hook all methods entered in the Hooks file, if found. 
Hooking a specific method when the **HooksRegistry** binary has no code to inject will have NO side effect on the game! The game will run a bit slower though..

## Build

Visual Studio 2017 has to be installed to build both projects. Required components are C# - and Unity development tools! Visual Studio 2017 Community edition is free to download and capable to perform the build.

1. Clone the repo;
2. Create a junction link between the solution folder and the game install path. See `/createJunction.bat`;
2. Open UnityHook solution file with Visual Studio;
3. Build project **Hooker**;
4. Build project **HookRegistry**;
5. All binary files can be found inside the `bin` folder of each project.

## Usage Example
> The example expects the example_hooks file to be used as of the latest commit, also the latest **HookRegistry** binary.

Effects of the example
- The game creates a non secure connection to the server (NOT through a TLS tunnel);
- All transferred network packets are being duplicated to another TCP 'dump'-stream.

What you need

- The **PATH** to **Hooker** compiled binaries. Refered to as {HOOKERPATH};
- The compiled binary **FILE** from **HookRegistry**. Referred to as {REGISTRY};
- The **PATH** to the game installation folder. Referred to as {GAMEDIR};
- The path to a hooks **FILE**, example_hooks as mentioned above. Referred to as {HOOKS}.
    
Steps

1. Call Hooker.exe;
```
{HOOKERPATH}\Hooker.exe hook -d "{GAMEDIR}" -h "{HOOKS}" -l "{REGISTRY}"
```
2. Verify that that Hooker did not encounter a problem;
    - Check the log output;
    - Requested methods are hooked;
    - Game assemblies are duplicated next to the original as {name}.original.dll -> Backup;
    - Patched assemblies are written to the game directory as {name}.out.dll;
    - Patched assemblies replaced the original assemblies as {name}.dll;
    - **HookRegistry** assembly, **and referenced assemblies**, are copied next to the game assemblies;
3. Run the game -> Watch the game log for lines starting with [HOOKER].

> To restore, run the command ```{HOOKERPATH}\Hooker.exe restore -d "{GAMEDIR}"```

## Remarks

* This project is intended to run within the context of the Unity Engine. If Unity Engine is *not* initialised when **HookRegistry** is initialised, then no hooks will run. Each method will perform as if unhooked when outside of the Unity Engine context.
