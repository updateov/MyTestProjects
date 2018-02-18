function this = filterByLag(this, minLag, maxLag)

    if (~exist('maxLag', 'var'))
        maxLag = inf;
    end
    if hasFhrPart2(this)
        keep = zeros(1, length(this));
        n = 0;
        for i = 1:length(this)
%             getLag(this.set{i})
            if isa(this.set{i}, 'decel2') && (getLag(this.set{i}) >= minLag) && (getLag(this.set{i}) < maxLag)
                n = n + 1;
                keep(n) = i;
            end
        end
        keep = keep(1:n);
        this = keepIndices(this, keep);
    end
    
return;