using System;

// ReSharper disable CheckNamespace
namespace Orientdb.Net.Purify
// ReSharper restore CheckNamespace
{
    public interface IPurifier
    {
        void Purify(Uri uri);
    }
}