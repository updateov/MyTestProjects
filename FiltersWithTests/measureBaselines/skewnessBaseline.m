function uSkew = skewnessBaseline(u, config)

nSamples = length(u);

uSignal = uniformSignal(u);
uSkew = getSamples(skewness(uSignal, config.SKEWNESS_WIDTH, config.MEDIAN_EXTRAP_METHOD));
uSkew(isnan(u)) = NaN;

return                                 