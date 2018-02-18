%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @uniformSignal/uniformSignal
%
%   Description:    
%		constructs a uniformSignal with the given (equally spaced) samples, 
%       sampling rate and extent
%
%   Signatures
%		this = signal(name, Fs, extent, samples);
%
%   Returns:
%     this			(uniformSignal)
%
%	History:
%		14 Aug 2001			PAW
%		20 Sept 2001		PAW 	create as class
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = uniformSignal(varargin)
switch nargin
case 0
%		this = uniformSignal
    parent = signal;
    this = class(struct([]), 'uniformSignal', parent);
case 1
   % if single argument of class signal, return it
   if (isa(varargin{1},'signal'))
       this = toUniformSignal(varargin{1});
   elseif (isa(varargin{1},'double'))
       parent = signal('', 0, 0, varargin{1}, []);
       this = class(struct([]), 'uniformSignal', parent);
   else
      error('Wrong argument type')
   end
case 4
   parent = signal(varargin{:}, []);
   this = class(struct([]), 'uniformSignal', parent);
otherwise
	error('Wrong number of input arguments')   
end
return
