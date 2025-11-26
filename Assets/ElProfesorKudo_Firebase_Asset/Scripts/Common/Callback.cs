namespace ElProfesorKudo.Firebase.Common
{
    public delegate void Callback();
    public delegate void Callback<T>(T arg1);
    public delegate void Callback<T, U>(T arg1, U arg2);
    public delegate void Callback<T, U, W>(T arg1, U arg2, W arg3);
}