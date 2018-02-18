%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   function @fhrPartSet/subsasgn.m
%
%   Description:    
%	   used for 
%           1) public assignment, e.g. 
%                anFhrPartSet1(i:i+j) = anFhrPartSet2(k:k+j)
%           2) assignment to this.set, e.g. 
%                this.set{i:j} = ..."
%
%	 Signatures:
%       subset = subsasgn(this,index)
%
%
%   Returns:
%
%	 $Revision $
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
function this = subsasgn(this, index, val)
switch index.type
case '{}'
    if isa(index.subs{:},'numeric') & length(index.subs{:}) == 1
        this.set{index.subs{:}}=val;
    else
    error('{} subscripting not supported for fhrPartSet');
end
case '()'
    if isempty(val)
        this.set(index.subs{:}) = [];
    else
        this.set(index.subs{:}) = getRawData(val);
    end
%fc = fieldcount(this.asset);
%     if (index.subs{:} <= fc)
%         this.asset = subsasgn(this.asset,index,val);
%     else
%         switch index.subs{:}-fc
%         case 1
%             this.num_shares = val;
%         case 2
%             this.share_price = val;
%         otherwise
%         error(['Index must be in the range 1 to ',num2str(fc + 2)])
%         end
%     end
case '.'
    switch index.subs
    case 'set'
        this.set = subsasgn(this.set, index, val);
    otherwise
        error('Invalid field assignment for fhrPartSet');
    end
end
