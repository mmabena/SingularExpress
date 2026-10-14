namespace SingularExpress.Api.Interfaces;

/// <summary>Creates random numeric PINs. An interface so tests can control the PIN.</summary>
public interface IPinGenerator
{
  string Generate(int length);
}
