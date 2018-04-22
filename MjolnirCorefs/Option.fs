module Mjolnir.Core.Option

open System

let map f (o:'a option) : 'b option =
  if o.IsNone then
    None
  else
    o.Value |> f |> Some

let fromNullable (o:Nullable<'a>) : 'a option =
  if o.HasValue then
    Some o.Value
  else None

let toNullable (o:'a option) : Nullable<'a> =
  if o.IsSome then
    o.Value |> Nullable
  else null |> Nullable