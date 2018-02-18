%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @intervals/construct
%
%   Description:    
%		constructs a pair of coordinates of the same (x1) dimension
%
%	 Signatures
%		this = intervals;           creates empty intervals
%		this = intervals(intervals);
%		this = intervals(x1, x2);	x1 contains the begin coord
%									x2 contains the end coord
%
%   Returns:
%     this	(intervals)
%
%	 $Revision $
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = intervals(varargin)
switch nargin
case 0
   % this = intervals
   this = init;

case 1
   % this = intervals(anintervals);
   if isa(varargin{1},'intervals')
      this = varargin{1};
    elseif isa(varargin{1},'double') & length(varargin{1}) == 2
      this = init;
      this.x1 = varargin{1}(1);
      this.x2 = varargin{1}(2);
      checkIntegrity(this);
    elseif isa(varargin{1},'logical')
      this = init;
      this = fromLogical(this, varargin{1});
    else
      error('Wrong argument type')
   end

case 2
   % this = intervals(x1, x2);
   if (isa(varargin{1},'double') & isa(varargin{2},'double'))
      this = init;
		this.x1 = varargin{1};
		this.x2 = varargin{2};
        checkIntegrity(this);
	else
      error('Wrong argument type')
   end

case 3
   % this = intervals(x1, x2);
   if (isa(varargin{1},'double') & isa(varargin{2},'double') & isa(varargin{3},'char'))
      this = init;
		this.x1 = varargin{1};
		this.x2 = varargin{2};
        this.name = varargin{3};
        checkIntegrity(this);
	else
      error('Wrong argument type')
   end
case 4
   % this = intervals(referenceTime, eventTime, duration, Ts);
   if (isa(varargin{1},'char') & isa(varargin{2},'cell') & ...
       isa(varargin{3},'double') & isa(varargin{4},'double'))
      this = init;
      this = fromTimeAndDuration(this, varargin{1}, varargin{2}, ...
                                       varargin{3}, varargin{4});
      checkIntegrity(this);
	else
      error('Wrong argument type')
   end
otherwise
	error('Wrong number of input arguments')   
end
return

function this = init() 
this.x1 = [];
this.x2 = [];
this.name = [];
this = class(this, 'intervals');
return

function checkIntegrity(this)
if length(this.x1) ~= length(this.x2)
    warning('Begin and end index lengths are not equal');
end
