module Mjolnir.Core.Option

open System
open System.Runtime.InteropServices

let map f (o:'a option) : 'b option =
  if o.IsNone then
    None
  else
    o.Value |> f |> Some

let ofNullType (o:'a) : 'a option when 'a : null =
  if o = null then
    None
  else Some o

let toNullType (o:'a option) : 'a =
  if o.IsNone then
    null
  else o.Value

// Source: http://www.fssnip.net/so/title/Make-option-type-usable-in-C
let IsSome o =
  match o with
  | Some _ -> true
  | None -> false
let IsNone o = not <| IsSome o
let TryGetValue o ([<Out>] value: byref<'t>) =
  match o with
  | Some v -> 
    value <- v
    true
  | None ->
    value <- Unchecked.defaultof<'t>
    false