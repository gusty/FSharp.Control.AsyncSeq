
namespace FSharp.Control

open System

/// An enumerator for pulling results asynchronously
type IAsyncEnumerator<'T> =
     abstract MoveNext : unit -> Async<'T option>
     inherit IDisposable

/// An asynchronous sequence represents a delayed computation that can be
/// started to give an enumerator for pulling results asynchronously
type IAsyncEnumerable<'T> = 
    abstract GetEnumerator : unit -> IAsyncEnumerator<'T>

/// An asynchronous sequence represents a delayed computation that can be
/// started to give an enumerator for pulling results asynchronously
type AsyncSeq<'T> = IAsyncEnumerable<'T>

[<RequireQualifiedAccess>]
module AsyncSeq = 
    /// Creates an empty asynchronous sequence that immediately ends.
    [<GeneralizableValueAttribute>]
    val empty<'T> : AsyncSeq<'T>

    /// Creates an asynchronous sequence that generates a single element and then ends.
    val singleton : v:'T -> AsyncSeq<'T>

    /// Generates a finite async sequence using the specified asynchronous initialization function.
    val initAsync : count: int64 -> mapping:(int64 -> Async<'T>) -> AsyncSeq<'T>

    /// Generates a finite async sequence using the specified initialization function.
    val init : count: int64 -> mapping:(int64 -> 'T) -> AsyncSeq<'T>

    /// Generates an infinite async sequence using the specified asynchronous initialization function.
    val initInfiniteAsync : mapping:(int64 -> Async<'T>) -> AsyncSeq<'T>

    /// Generates an infinite async sequence using the specified initialization function.
    val initInfinite : mapping:(int64 -> 'T) -> AsyncSeq<'T>

    /// Generates an async sequence using the specified asynchronous generator function.
    val unfoldAsync : generator:('State -> Async<('T * 'State) option>) -> state:'State -> AsyncSeq<'T>

    /// Generates an async sequence using the specified generator function.
    val unfold : generator:('State -> ('T * 'State) option) -> state:'State -> AsyncSeq<'T>

    /// Creates an async sequence which repeats the specified value the indicated number of times.
    val replicate : count: int -> v:'T -> AsyncSeq<'T>

    /// Creates an infinite async sequence which repeats the specified value.
    val replicateInfinite : v:'T -> AsyncSeq<'T>

    /// Creates an infinite async sequence which repeatedly evaluates and emits the specified async value.
    val replicateInfiniteAsync : v:Async<'T> -> AsyncSeq<'T>

    /// Returns an async sequence which emits an element on a specified period.
    val intervalMs : periodMs:int -> AsyncSeq<DateTime>

    /// Yields all elements of the first asynchronous sequence and then 
    /// all elements of the second asynchronous sequence.
    val append : seq1:AsyncSeq<'T> -> seq2:AsyncSeq<'T> -> AsyncSeq<'T>

    /// Computation builder that allows creating of asynchronous 
    /// sequences using the 'asyncSeq { ... }' syntax
    type AsyncSeqBuilder =

        /// Internal use only
        new : unit -> AsyncSeqBuilder

        /// Implements binding for the asyncSeq computation builder.
        member Bind : source:Async<'T> * body:('T -> AsyncSeq<'U>) -> AsyncSeq<'U>

        /// Implements sequential composition for the asyncSeq computation builder.
        member Combine : seq1:AsyncSeq<'T> * seq2:AsyncSeq<'T> -> AsyncSeq<'T>

        /// Implements delay for the asyncSeq computation builder.
        member Delay : f:(unit -> AsyncSeq<'T>) -> AsyncSeq<'T>

        /// For loop that iterates over a synchronous sequence (and generates
        /// all elements generated by the asynchronous body)
        member For : source:seq<'T> * action:('T -> AsyncSeq<'TResult>) -> AsyncSeq<'TResult>

        /// Implements "for" loops for the asyncSeq computation builder.
        ///
        /// Asynchronous for loop - for all elements from the input sequence,
        /// generate all elements produced by the body (asynchronously). See
        /// also the AsyncSeq.collect function.
        member For : source:AsyncSeq<'T> * action:('T -> AsyncSeq<'TResult>) -> AsyncSeq<'TResult>

        /// Implements "return" for the asyncSeq computation builder.
        member Return : 'unit -> AsyncSeq<'T> 

        /// Implements "try-finally" for the asyncSeq computation builder.
        member TryFinally : body:AsyncSeq<'T> * compensation:(unit -> unit) -> AsyncSeq<'T>

        /// Implements "try-with" for the asyncSeq computation builder.
        member TryWith : body:AsyncSeq<'T> * handler:(exn -> AsyncSeq<'T>) -> AsyncSeq<'T>

        /// Implements "use" for the asyncSeq computation builder.
        member Using : resource:'T * binder:('T -> AsyncSeq<'U>) -> AsyncSeq<'U> when 'T :> System.IDisposable

        /// Implements "while" for the asyncSeq computation builder.
        member While : guard:(unit -> bool) * body:AsyncSeq<'T> -> AsyncSeq<'T>

        /// Implements "yield" for the asyncSeq computation builder.
        member Yield : value:'T -> AsyncSeq<'T>

        /// Implements "yield!" for the asyncSeq computation builder.
        member YieldFrom : source:AsyncSeq<'T> -> AsyncSeq<'T>

        /// Implements empty for the asyncSeq computation builder.
        member Zero : unit -> AsyncSeq<'T>


    /// Creates an asynchronous sequence that iterates over the given input sequence.
    /// For every input element, it calls the the specified function and iterates
    /// over all elements generated by that asynchronous sequence.
    /// This is the 'bind' operation of the computation expression (exposed using
    /// the 'for' keyword in asyncSeq computation).
    val collect : mapping:('T -> AsyncSeq<'TResult>) -> source:AsyncSeq<'T> -> AsyncSeq<'TResult>

    /// Builds a new asynchronous sequence whose elements are generated by 
    /// applying the specified function to all elements of the input sequence.
    ///
    /// The specified function is asynchronous (and the input sequence will
    /// be asked for the next element after the processing of an element completes).
    val mapAsync : mapping:('T -> Async<'TResult>) -> source:AsyncSeq<'T> -> AsyncSeq<'TResult>

    /// Asynchronously iterates over the input sequence and generates 'x' for 
    /// every input element for which the specified asynchronous function 
    /// returned 'Some(x)' 
    ///
    /// The specified function is asynchronous (and the input sequence will
    /// be asked for the next element after the processing of an element completes).
    val chooseAsync : mapping:('T -> Async<'R option>) -> source:AsyncSeq<'T> -> AsyncSeq<'R>

    /// Builds a new asynchronous sequence whose elements are those from the
    /// input sequence for which the specified function returned true.
    ///
    /// The specified function is asynchronous (and the input sequence will
    /// be asked for the next element after the processing of an element completes).
    val filterAsync : predicate:('T -> Async<bool>) -> source:AsyncSeq<'T> -> AsyncSeq<'T>

    /// Asynchronously returns the last element that was generated by the
    /// given asynchronous sequence (or the specified default value).
    val lastOrDefault : ``default``:'T -> source:AsyncSeq<'T> -> Async<'T>

    /// Asynchronously returns the last element that was generated by the
    /// given asynchronous sequence (or None if the sequence is empty).
    val tryLast : source:AsyncSeq<'T> -> Async<'T option>

    /// Asynchronously returns the first element that was generated by the
    /// given asynchronous sequence (or the specified default value).
    val firstOrDefault : ``default``:'T -> source:AsyncSeq<'T> -> Async<'T>

    /// Asynchronously returns the first element that was generated by the
    /// given asynchronous sequence (or None if the sequence is empty).
    val tryFirst : source:AsyncSeq<'T> -> Async<'T option>

    /// Aggregates the elements of the input asynchronous sequence using the
    /// specified 'aggregation' function. The result is an asynchronous 
    /// sequence of intermediate aggregation result.
    ///
    /// The aggregation function is asynchronous (and the input sequence will
    /// be asked for the next element after the processing of an element completes).
    val scanAsync : folder:('State -> 'T -> Async<'State>) -> state:'State -> source:AsyncSeq<'T> -> AsyncSeq<'State>

    /// Iterates over the input sequence and calls the specified asynchronous function for
    /// every value. The input sequence will be asked for the next element after 
    /// the processing of an element completes.
    val iterAsync : action:('T -> Async<unit>) -> source:AsyncSeq<'T> -> Async<unit>

    /// Returns an asynchronous sequence that returns pairs containing an element
    /// from the input sequence and its predecessor. Empty sequence is returned for
    /// singleton input sequence.
    val pairwise : source:AsyncSeq<'T> -> AsyncSeq<'T * 'T>

    /// Asynchronously aggregate the elements of the input asynchronous sequence using the
    /// specified asynchronous 'aggregation' function. 
    val foldAsync : folder:('State -> 'T -> Async<'State>) -> state:'State -> source:AsyncSeq<'T> -> Async<'State>

    /// Asynchronously aggregate the elements of the input asynchronous sequence using the
    /// specified 'aggregation' function. 
    val fold : folder:('State -> 'T -> 'State) -> state:'State -> source:AsyncSeq<'T> -> Async<'State>

    /// Asynchronously sum the elements of the input asynchronous sequence using the specified function. 
    val inline sum : source:AsyncSeq< ^T > -> Async< ^T>
                                when ^T : (static member ( + ) : ^T * ^T -> ^T) 
                                and  ^T : (static member Zero : ^T)

    /// Asynchronously determine if the sequence contains the given value
    val contains : value:'T -> source:AsyncSeq<'T> -> Async<bool> when 'T : equality

    /// Asynchronously pick a value from a sequence based on the specified chooser function.
    val tryPickAsync : chooser:('T -> Async<'TResult option>) -> source:AsyncSeq<'T> -> Async<'TResult option>

    /// Asynchronously pick a value from a sequence based on the specified chooser function.
    val tryPick : chooser:('T -> 'TResult option) -> source:AsyncSeq<'T> -> Async<'TResult option>

    /// Asynchronously pick a value from a sequence based on the specified chooser function.
    /// Raises KeyNotFoundException if the chooser function can't find a matching key.
    val pickAsync : chooser:('T -> Async<'TResult option>) -> source:AsyncSeq<'T> -> Async<'TResult>

    /// Asynchronously pick a value from a sequence based on the specified chooser function.
    /// Raises KeyNotFoundException if the chooser function can't find a matching key.
    val pick : chooser:('T -> 'TResult option) -> source:AsyncSeq<'T> -> Async<'TResult>

    /// Asynchronously find the first value in a sequence for which the predicate returns true
    val tryFind : predicate:('T -> bool) -> source:AsyncSeq<'T> -> Async<'T option>

    /// Asynchronously determine if there is a value in the sequence for which the predicate returns true
    val exists : predicate:('T -> bool) -> source:AsyncSeq<'T> -> Async<bool>

    /// Asynchronously determine if the predicate returns true for all values in the sequence 
    val forall : predicate:('T -> bool) -> source:AsyncSeq<'T> -> Async<bool>

    /// Return an asynchronous sequence which, when iterated, includes an integer indicating the index of each element in the sequence.
    val indexed : source:AsyncSeq<'T> -> AsyncSeq<int64 * 'T>

    /// Asynchronously determine the number of elements in the sequence
    val length : source:AsyncSeq<'T> -> Async<int64>

    /// Same as AsyncSeq.scanAsync, but the specified function is synchronous.
    val scan : folder:('State -> 'T -> 'State) -> state:'State -> source:AsyncSeq<'T> -> AsyncSeq<'State>

    /// Same as AsyncSeq.mapAsync, but the specified function is synchronous.
    val map : folder:('T -> 'U) -> source:AsyncSeq<'T> -> AsyncSeq<'U>

    /// Iterates over the input sequence and calls the specified function for
    /// every value.
    val iter : action:('T -> unit) -> source:AsyncSeq<'T> -> Async<unit>

    /// Asynchronously iterates over the input sequence and generates 'x' for 
    /// every input element for which the specified function 
    /// returned 'Some(x)' 
    val choose : chooser:('T -> 'U option) -> source:AsyncSeq<'T> -> AsyncSeq<'U>

    /// Same as AsyncSeq.filterAsync, but the specified predicate is synchronous
    /// and processes the input element immediately.
    val filter : predicate:('T -> bool) -> source:AsyncSeq<'T> -> AsyncSeq<'T>

    /// Creates an asynchronous sequence that lazily takes element from an
    /// input synchronous sequence and returns them one-by-one.
    val ofSeq : source:seq<'T> -> AsyncSeq<'T>

    /// Converts observable to an asynchronous sequence. Values that are produced
    /// by the observable while the asynchronous sequence is blocked are stored to 
    /// an unbounded buffer and are returned as next elements of the async sequence.
    val ofObservableBuffered : source:System.IObservable<'T> -> AsyncSeq<'T>

    [<System.Obsolete("Please use AsyncSeq.ofObservableBuffered. The original AsyncSeq.ofObservable doesn't guarantee that the asynchronous sequence will return all values produced by the observable",true) >]
    val ofObservable : source:System.IObservable<'T> -> AsyncSeq<'T>

    /// Converts asynchronous sequence to an IObservable<_>. When the client subscribes
    /// to the observable, a new copy of asynchronous sequence is started and is 
    /// sequentially iterated over (at the maximal possible speed). Disposing of the 
    /// observer cancels the iteration over asynchronous sequence. 
    val toObservable : source:AsyncSeq<'T> -> System.IObservable<'T>

    /// Converts asynchronous sequence to a synchronous blocking sequence.
    /// The elements of the asynchronous sequence are consumed lazily.
    val toBlockingSeq : source:AsyncSeq<'T> -> seq<'T>

    /// Create a new asynchronous sequence that caches all elements of the 
    /// sequence specified as the input. When accessing the resulting sequence
    /// multiple times, the input will still be evaluated only once
    val cache : source:AsyncSeq<'T> -> AsyncSeq<'T>

    /// Threads a state through the mapping over an async sequence using an async function.
    val threadStateAsync : folder:('State -> 'T -> Async<'U * 'State>) -> st:'State -> source:AsyncSeq<'T> -> AsyncSeq<'U>

    /// Combines two asynchronous sequences into a sequence of pairs. 
    /// The values from sequences are retrieved in parallel. 
    /// The resulting sequence stops when either of the argument sequences stop.
    val zip : input1:AsyncSeq<'T1> -> input2:AsyncSeq<'T2> -> AsyncSeq<'T1 * 'T2>

    /// Combines two asynchronous sequences using the specified function.
    /// The values from sequences are retrieved in parallel.
    /// The resulting sequence stops when either of the argument sequences stop.
    val zipWithAsync : mapping:('T1 -> 'T2 -> Async<'U>) -> source1:AsyncSeq<'T1> -> source2:AsyncSeq<'T2> -> AsyncSeq<'U>

    /// Combines two asynchronous sequences using the specified function.
    /// The values from sequences are retrieved in parallel.
    /// The resulting sequence stops when either of the argument sequences stop.
    val zipWith : mapping:('T1 -> 'T2 -> 'U) -> source1:AsyncSeq<'T1> -> source2:AsyncSeq<'T2> -> AsyncSeq<'U>

    /// Builds a new asynchronous sequence whose elements are generated by 
    /// applying the specified function to all elements of the input sequence.
    ///
    /// The specified function is asynchronous (and the input sequence will
    /// be asked for the next element after the processing of an element completes).
    val mapiAsync : mapping:(int64 -> 'T -> Async<'U>) -> source:AsyncSeq<'T> -> AsyncSeq<'U>

    [<System.Obsolete("Renamed to mapiAsync") >]
    val zipWithIndexAsync : mapping:(int64 -> 'T -> Async<'U>) -> source:AsyncSeq<'T> -> AsyncSeq<'U>

    /// Feeds an async sequence of values into an async sequence of async functions.
    val zappAsync : functions:AsyncSeq<('T -> Async<'U>)> -> source:AsyncSeq<'T> -> AsyncSeq<'U>

    /// Feeds an async sequence of values into an async sequence of functions.
    val zapp : functions:AsyncSeq<('T -> 'U)> -> source:AsyncSeq<'T> -> AsyncSeq<'U>

    /// Merges two async sequences using the specified combine function. The resulting async sequence produces an element when either
    /// input sequence produces an element, passing the new element from the emitting sequence and the previously emitted element from the other sequence.
    /// If either of the input sequences is empty, the resulting sequence is empty.
    val combineLatestWithAsync : combine:('T -> 'U -> Async<'V>) -> source1:AsyncSeq<'T> -> source2:AsyncSeq<'U> -> AsyncSeq<'V>

    /// Merges two async sequences using the specified combine function. The resulting async sequence produces an element when either
    /// input sequence produces an element, passing the new element from the emitting sequence and the previously emitted element from the other sequence.
    /// If either of the input sequences is empty, the resulting sequence is empty.
    val combineLatestWith : combine:('T -> 'U -> 'V) -> source1:AsyncSeq<'T> -> source2:AsyncSeq<'U> -> AsyncSeq<'V>

    /// Merges two async sequences. The resulting async sequence produces an element when either
    /// input sequence produces an element, passing the new element from the emitting sequence and the previously emitted element from the other sequence.
    /// If either of the input sequences is empty, the resulting sequence is empty.
    val combineLatest : source1:AsyncSeq<'a> -> source2:AsyncSeq<'b> -> AsyncSeq<'a * 'b>

    /// Traverses an async sequence an applies to specified function such that if None is returned the traversal short-circuits
    /// and None is returned as the result. Otherwise, the entire sequence is traversed and the result returned as Some.
    val traverseOptionAsync : mapping:('T -> Async<'U option>) -> source:AsyncSeq<'T> -> Async<AsyncSeq<'U> option>

    /// Traverses an async sequence an applies to specified function such that if Choice2Of2 is returned the traversal short-circuits
    /// and Choice2Of2 is returned as the result. Otherwise, the entire sequence is traversed and the result returned as Choice1Of2.
    val traverseChoiceAsync : mapping:('T -> Async<Choice<'U,'e>>) -> source:AsyncSeq<'T> -> Async<Choice<AsyncSeq<'U>,'e>>

    /// Returns elements from an asynchronous sequence while the specified 
    /// predicate holds. The predicate is evaluated asynchronously.
    val takeWhileAsync : predicate:('T -> Async<bool>) -> source:AsyncSeq<'T> -> AsyncSeq<'T>

    /// Returns elements from the argument async sequence until the specified signal completes or
    /// the sequences completes.
    val takeUntilSignal : signal:Async<unit> -> source:AsyncSeq<'T> -> AsyncSeq<'T>

    [<System.Obsolete("Renamed to takeUntilSignal") >]
    val takeUntil : signal:Async<unit> -> source:AsyncSeq<'T> -> AsyncSeq<'T>

    /// Skips elements from an asynchronous sequence while the specified 
    /// predicate holds and then returns the rest of the sequence. The 
    /// predicate is evaluated asynchronously.
    val skipWhileAsync : predicate:('T -> Async<bool>) -> source:AsyncSeq<'T> -> AsyncSeq<'T>

    /// Skips elements from an async sequence until the specified signal completes.
    val skipUntilSignal : signal:Async<unit> -> source:AsyncSeq<'T> -> AsyncSeq<'T>

    [<System.Obsolete("Renamed to skipUntilSignal") >]
    val skipUntil : signal:Async<unit> -> source:AsyncSeq<'T> -> AsyncSeq<'T>

    /// Returns elements from an asynchronous sequence while the specified 
    /// predicate holds. The predicate is evaluated synchronously.
    val takeWhile : predicate:('T -> bool) -> source:AsyncSeq<'T> -> AsyncSeq<'T>

    /// Skips elements from an asynchronous sequence while the specified 
    /// predicate holds and then returns the rest of the sequence. The 
    /// predicate is evaluated asynchronously.
    val skipWhile : predicate:('T -> bool) -> source:AsyncSeq<'T> -> AsyncSeq<'T>

    /// Returns the first N elements of an asynchronous sequence
    val take : count:int -> source:AsyncSeq<'T> -> AsyncSeq<'T>

    /// Skips the first N elements of an asynchronous sequence and
    /// then returns the rest of the sequence unmodified.
    val skip : count:int -> source:AsyncSeq<'T> -> AsyncSeq<'T>

    /// Creates an async computation which iterates the AsyncSeq and collects the output into an array.
    val toArrayAsync : source:AsyncSeq<'T> -> Async<'T []>

    /// Creates an async computation which iterates the AsyncSeq and collects the output into a list.
    val toListAsync : source:AsyncSeq<'T> -> Async<'T list>

    /// Synchronously iterates the AsyncSeq and collects the output into a list.
    val toList : source:AsyncSeq<'T> -> 'T list

    /// Synchronously iterates the AsyncSeq and collects the output into an array.
    val toArray : source:AsyncSeq<'T> -> 'T []

    /// Flattens an AsyncSeq of sequences.
    val concatSeq : source:AsyncSeq<#seq<'T>> -> AsyncSeq<'T>

    /// Interleaves two async sequences of the same type into a resulting sequence. The provided
    /// sequences are consumed in lock-step.
    val interleave : source1:AsyncSeq<'T> -> source2:AsyncSeq<'T> -> AsyncSeq<'T>

    /// Interleaves two async sequences into a resulting sequence. The provided
    /// sequences are consumed in lock-step.
    val interleaveChoice  : source1:AsyncSeq<'T1> -> source2:AsyncSeq<'T2> -> AsyncSeq<Choice<'T1,'T2>>

    /// Buffer items from the async sequence into buffers of a specified size.
    /// The last buffer returned may be less than the specified buffer size.
    val bufferByCount : bufferSize:int -> source:AsyncSeq<'T> -> AsyncSeq<'T []>

    /// Buffer items from the async sequence until a specified buffer size is reached or a specified amount of time is elapsed.
    val bufferByCountAndTime : bufferSize:int -> timeoutMs:int -> source:AsyncSeq<'T> -> AsyncSeq<'T []>

    /// Merges two async sequences into an async sequence non-deterministically.
    /// The resulting async sequence produces elements when any argument sequence produces an element.
    val mergeChoice: source1:AsyncSeq<'T1> -> source2:AsyncSeq<'T2> -> AsyncSeq<Choice<'T1,'T2>>

    /// Merges two async sequences of the same type into an async sequence non-deterministically.
    /// The resulting async sequence produces elements when any argument sequence produces an element.
    val merge: source1:AsyncSeq<'T> -> source2:AsyncSeq<'T> -> AsyncSeq<'T>

    /// Merges all specified async sequences into an async sequence non-deterministically.
    /// The resulting async sequence produces elements when any argument sequence produces an element.
    val mergeAll : sources:AsyncSeq<'T> list -> AsyncSeq<'T>

    /// Returns an async sequence which contains no contiguous duplicate elements based on the specified comparison function.
    val distinctUntilChangedWithAsync : mapping:('T -> 'T -> Async<bool>) -> source:AsyncSeq<'T> -> AsyncSeq<'T>

    /// Returns an async sequence which contains no contiguous duplicate elements based on the specified comparison function.
    val distinctUntilChangedWith : mapping:('T -> 'T -> bool) -> source:AsyncSeq<'T> -> AsyncSeq<'T>

    /// Returns an async sequence which contains no contiguous duplicate elements.
    val distinctUntilChanged : source:AsyncSeq<'T> -> AsyncSeq<'T> when 'T : equality

    [<System.Obsolete("Use .GetEnumerator directly") >]
    val getIterator : source:AsyncSeq<'T> -> (unit -> Async<'T option>)

    /// Builds a new asynchronous sequence whose elements are generated by 
    /// applying the specified function to all elements of the input sequence.
    ///
    /// The function is applied to elements in order and results are emitted in order,
    /// but in parallel, without waiting for a prior mapping operation to complete. 
    /// Parallelism is bound by the ThreadPool.
    val mapAsyncParallel : mapping:('T -> Async<'U>) -> s:AsyncSeq<'T> -> AsyncSeq<'U>

    /// Applies a key-generating function to each element and returns an async sequence containing unique keys
    /// and async sequences containing elements corresponding to the key.
    /// 
    /// Note that the resulting async sequence has to be processed in parallel (e.g AsyncSeq.mapAsyncParallel) becaused
    /// completion of sub-sequences depends on completion of other sub-sequences.
    val groupByAsync<'T, 'Key when 'Key : equality> : projection:('T -> Async<'Key>) -> source:AsyncSeq<'T> -> AsyncSeq<'Key * AsyncSeq<'T>>

    /// Applies a key-generating function to each element and returns an async sequence containing unique keys
    /// and async sequences containing elements corresponding to the key.
    ///
    /// Note that the resulting async sequence has to be processed in parallel (e.g AsyncSeq.mapAsyncParallel) becaused
    /// completion of sub-sequences depends on completion of other sub-sequences.
    val groupBy<'T, 'Key when 'Key : equality> : projection:('T -> 'Key) -> source:AsyncSeq<'T> -> AsyncSeq<'Key * AsyncSeq<'T>>

/// An automatically-opened module that contains the `asyncSeq` builder and an extension method 
[<AutoOpen>]
module AsyncSeqExtensions = 
    /// Builds an asynchronous sequence using the computation builder syntax
    val asyncSeq : AsyncSeq.AsyncSeqBuilder

    /// Converts asynchronous sequence to a synchronous blocking sequence.
    /// The elements of the asynchronous sequence are consumed lazily.
    type AsyncBuilder with
      member For : seq:AsyncSeq<'T> * action:('T -> Async<unit>) -> Async<unit>

module Seq = 
    /// Converts asynchronous sequence to a synchronous blocking sequence.
    /// The elements of the asynchronous sequence are consumed lazily.
    val ofAsyncSeq : source:AsyncSeq<'T> -> seq<'T>


/// An async sequence source produces async sequences.
type AsyncSeqSrc<'T>

/// Operations on async sequence sources.
module AsyncSeqSrc =
    
  /// Creates a new async sequence source.
  val create : unit -> AsyncSeqSrc<'T>

  /// Causes any async sequences created before the call to yield the item.
  val put : item:'T -> src:AsyncSeqSrc<'T> -> unit

  /// Closes the async sequence source casuing any created async sequences to complete.
  val close : src:AsyncSeqSrc<'T> -> unit

  /// Causes async sequence created before the call to raise an exception.
  val error : exn:exn -> src:AsyncSeqSrc<'T> -> unit

  /// Creates an async sequence which yields values as they are put into the source and terminates
  /// when the source is closed. This sequence will yield items starting with the next put.
  /// Many async sequences can be created from once source.
  val toAsyncSeq : src:AsyncSeqSrc<'T> -> AsyncSeq<'T>