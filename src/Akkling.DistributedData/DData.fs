﻿//-----------------------------------------------------------------------
// <copyright file="DData.fs" company="Akka.NET Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/akka.net>
//     Copyright (C) 2016 Bartosz Sypytkowski <gttps://github.com/Horusiath>
// </copyright>
//-----------------------------------------------------------------------

[<AutoOpen>]
module Akkling.DistributedData.DData

open System
open System.Collections.Generic
open Akka.Cluster
open Akka.DistributedData
open Akkling

let update modify consistency init key = Dsl.Update(key, init, consistency, System.Func<_,_>(modify))
let get consistency key = Dsl.Get(key, consistency)
let delete consistency key = Dsl.Delete(key, consistency)

let getReplicator (system: Akka.Actor.ActorSystem) : IActorRef<IReplicatorMessage> = 
    DistributedData.Get(system).Replicator |> typed
    
let (|DataDeleted|_|) (msg: obj) =
    match msg with
    | :? DataDeleted as d -> Some d.Key
    | _ -> None
    
let (|GetSuccess|_|) (msg: obj) : (IKey * #IReplicatedData * obj) option =
    match msg with
    | :? GetSuccess as s when (s.Data :? 't) -> Some (s.Key, s.Data :?> 't, s.Request)
    | _ -> None
    
let (|GetFailure|_|) (msg: obj) =
    match msg with
    | :? GetFailure as d -> Some (d.Key, d.Request)
    | _ -> None
    
let (|NotFound|_|) (msg: obj) =
    match msg with
    | :? NotFound as d -> Some (d.Key, d.Request)
    | _ -> None
    
let (|UpdateSuccess|_|) (msg: obj) =
    match msg with
    | :? UpdateSuccess as d -> Some (d.Key, d.Request)
    | _ -> None
    
let (|ModifyFailure|_|) (msg: obj) =
    match msg with
    | :? ModifyFailure as d -> Some (d.Key, d.Cause, d.Request)
    | _ -> None
    
let (|UpdateTimeout|_|) (msg: obj) =
    match msg with
    | :? UpdateTimeout as d -> Some (d.Key, d.Cause, d.Request)
    | _ -> None
    
let (|DeleteSuccess|_|) (msg: obj) =
    match msg with
    | :? DeleteSuccess as d -> Some (d.Key, d.AlreadyDeleted)
    | _ -> None
    
let (|DeleteFailure|_|) (msg: obj) =
    match msg with
    | :? ReplicationDeletedFailure as d -> Some (d.Key, d.AlreadyDeleted)
    | _ -> None

let (|Changed|_|) (msg: obj) : (IKey * #IReplicatedData) option=
    match msg with
    | :? Changed as d when (d.Data :? 't) -> Some (d.Key, d.Data :?> 't)
    | _ -> None
    
let (|GetKeysIdsResult|_|) (msg: obj) =
    match msg with
    | :? GetKeysIdsResult as d -> Some (d.Keys |> Set.ofSeq)
    | _ -> None