﻿namespace StoryboardSystem.Core; 

internal readonly struct Indexer {
    public object Token { get; }

    public Indexer(object token) {
        Token = token;
    }
}