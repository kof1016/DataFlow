﻿using System;

namespace Gateway.Synchronize
{
    public interface IProvider
    {
        IGhost[] Ghosts { get; }

        void Add(IGhost entity);

        void Remove(Guid id);

        IGhost Ready(Guid id);

        void ClearGhosts();
    }
}