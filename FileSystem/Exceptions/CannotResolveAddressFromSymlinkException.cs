﻿using System;

namespace FileSystem
{
    public class CannotResolveAddressFromSymlinkException : Exception
    {
        private const string DefaultMessage = "Can't resolve address from symlink";

        public CannotResolveAddressFromSymlinkException(string symlinkContentAddress) : base(
            $"{DefaultMessage}\n Address = {symlinkContentAddress}") { }
    }
}