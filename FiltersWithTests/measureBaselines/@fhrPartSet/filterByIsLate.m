%%%%%%%%%%%%%%%%%%%%%
%
%   filterByIsLate - only applies to fhrPart2 - checks 'isLate' flag
%   Intended for filtering variable decels w/ late timing
%
%%%%%%%%%%%%%%%

function this = filterByIsLate(this)

    if hasFhrPart2(this)
        
        keep = zeros(1, length(this));
        n = 0;
        for i = 1:length(this)
            if isa(this.set{i}, 'decel2') && (isLate(this.set{i}))
                n = n + 1;
                keep(n) = i;
            end
        end
        keep = keep(1:n);
    
        this = keepIndices(this, keep);
    end
    
return;
    