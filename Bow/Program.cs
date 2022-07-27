if (args.Length > 0)
{
    new Bow(args[0], false, args.Contains("--debug")).Run();
}
else
{
    Bow.RunShell();
}
