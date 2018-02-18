%%%%%%%%%%%%%%%%%%%%%%%%%%
%
%   returns true if interval2 intersect any interval in this
%
%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

function rc = doesIntersect(this, interval2)

    if (strcmp(class(interval2, fhrPart)))
        interval2 = toIntervals(fhrPartSet(fhrPart))
    end
    
    intX1 = getX1(interval2);
    intX2 = getX2(interval2);
    
    for i = 1:size(this)
        x1 = getX1(this.set{i});
        x2 = getX2(this.set{i});
        if x2 < intX1
            rc = 0;
        else
            if x2 < intX2
                rc = 1;
                return;
            else
                if x1 > intX2
                    rc = 0;
                else
                    rc = 1;
                    return;
                end
            end
        end
    end
    
return;
        