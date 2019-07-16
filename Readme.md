# ModFileCore

```c#
TmodFile tmf = new TmodFile(path);
using (tmf.Open()) 
{
    // tmf.Upgrade();
    // tmf.Downgrade();
    tmf.ReplaceFile("Windows.dll", data);
}
tmf.Save("new.tmod");
```

Able to load tmod files of all versions, and can upgrade or downgrade tmod file.

## Getting files

```c#
TmodFile tmf = new TmodFile(file);
byte[] data;
using (tmf.Open())
{
    data = tmf.GetTrueBytes("Name");
}
```

`GetTrueBytes` gets the bytes after decompression, and `GetRawBytes` gets the bytes before decompression.

## Replacing files

```c#
TmodFile tmf = new TmodFile(file);
byte[] data = new byte[999];  // some data
using (tmf.Open())
{
    data = tmf.ReplaceFile("Name", data);
}
```

## Saving tmod file

```c#
tmf.Save();
```

If ModLoader version of the tmod file is less than `0.11`, then automatically saves to the old format, and vice versa.

## Upgrade tmod file

```c#
TmodFile tmf = new TmodFile(file);
using (tmf.Open())
{
    tmf.Upgrade();
}
tmf.Save("New.tmod");
```

## Downgrade tmod file

```c#
TmodFile tmf = new TmodFile(file);
using (tmf.Open())
{
    tmf.Downgrade();
}
tmf.Save("New.tmod");
```