﻿namespace WasteFree.Business.Abstractions.Messaging;

public interface ICommand : IBaseCommand;

public interface ICommand<TResponse> : IBaseCommand;

public interface IBaseCommand;