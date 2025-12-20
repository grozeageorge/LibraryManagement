// <copyright file="ReaderType.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

namespace Library.Domain.Entities
{
    /// <summary>
    /// Reader type entity that represents different types of readers in the library system.
    /// </summary>
    public enum ReaderType
    {
        /// <summary>
        /// The standard reader type.
        /// </summary>
        Standard = 0,

        /// <summary>
        /// A library staff member with extended privileges.
        /// </summary>
        Librarian = 1,
    }
}
