﻿//----------------------------------------------------------------------------
//
// Copyright (c) 2014
//
//    Ryan Riley (@panesofglass) and Andrew Cherry (@kolektiv)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
//----------------------------------------------------------------------------

[<AutoOpen>]
module Freya.TodoBackend.Prelude

open System.IO
open System.Text
open Fleece
open Freya.Core
open Freya.Core.Operators
open Freya.Machine
open Freya.Types.Cors
open Freya.Types.Http
open Freya.Types.Language

(* Utility

   Useful functions that it's handy to have around but not defined
   elsewhere in F# *)

let tuple x y =
    x, y

(* Presets

   It's often useful/neater when working with freya to write some shorthand
   properties for some of the arguments to freyaMachine computation operations.
   It saves having to use (returnM ...) in multiple places within the
   computation expression, which can reduce duplication and help readability. *)

let corsOrigins =
    returnM AccessControlAllowOriginRange.Any

let corsHeaders =
    returnM [ "accept"; "content-type" ]

let en =
    returnM [ LanguageTag.Parse "en" ]

let json =
    returnM [ MediaType.JSON ]

let utf8 =
    returnM [ Charset.UTF8 ]

(* Request Body Helper

   Freya doesn't provide built-in ways of extracting data from the body of
   a request, as it's usually very specific to an application, and the Freya
   way is to let the developer choose the most suitable approach.

   We've used
   Fleece in this example, so we can use that to define the body function
   below, which (following from Fleece) uses static inference to determine
   the type of return value needed. *)

let readStream (x: Stream) =
    use reader = new StreamReader (x)
    reader.ReadToEnd ()

let readBody =
    readStream <!> getLM Request.body

let inline body () =
    (function | Choice1Of2 x -> Some x | _ -> None) <!> (parseJSON <!> readBody)

(* Content Negotiation/Representation Helper

   Freya is also agnostic about data serialization in the response direction as
   well, believing it to be a choice for the developer.

   Here we've taken a simple approach, defining a function which always returns
   UTF-8 encoded JSON, English language, provided that the argument can
   be serialized to JSON using Fleece. *)

let inline represent x =
    { Metadata =
        { Charset = Some Charset.UTF8
          Encodings = None
          MediaType = Some MediaType.JSON
          Languages = Some [ LanguageTag.Parse "en" ] }
      Data = (toJSON >> string >> Encoding.UTF8.GetBytes) x }