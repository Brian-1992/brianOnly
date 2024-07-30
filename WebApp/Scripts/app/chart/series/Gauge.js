/**
 * This class inherit Ext.chart.series.Gauge. 
 * Via override the {@link Ext.chart.series.Gauge#drawSeries drawSeries} Method to create customized appearance. 
 */
Ext.define('MMIS.chart.series.Gauge', {
    extend: 'Ext.chart.series.Gauge',

    alias: 'series.mmisgauge',

    /**
     * @cfg {String[]} BackplaneBaseColors
     * The colors of backplan
     */
    backplaneBaseColors: ['#fff', '#ddd'],

    /**
     * @cfg {Number} fragmentWidth
     * The width of each graduate
     */
    fragmentWidth: 18,

    /**
     * @cfg {object} graduate
     */
    graduate: {
        warnDegree: 30,
        dangerDegree: 10
    },

    /**
     * @cfg {String} safeGraduateColor
     */
    safeGraduateColor: "#00B366",

    /**
    * @cfg {String} warnGraduateColor
    */
    warnGraduateColor: "#FF9900",

    /**
    * @cfg {String} dangerGraduateColor
    */
    dangerGraduateColor: "#FF0000",

    drawPointerAxis: function () {
        var me = this,
            spriteConfig = {
                type: "circle",
                x: me.centerX,
                y: me.centerY,
                radius: 10,
                fill: '#ccc',
                stroke: 'black',
                'stroke-width': 1
            };

        if (me.chart.resizing && me.pointerAxisSprite) {
            me.pointerAxisSprite.destroy();
            delete me.pointerAxisSprite;
        }
        if (!me.pointerAxisSprite) {
            me.pointerAxisSprite = me.chart.surface.add(spriteConfig).redraw();
        } else {
            me.pointerAxisSprite.setAttributes(spriteConfig, true);
        }
    },

    movePointerToCurrectPostion: function (value, minimum, maximum) {
        var me = this,
            rotateConfig = {
                rotate: {
                    x: 0,
                    y: 0,
                    degrees: 180 * (value - minimum) / (maximum - minimum)
                }
            };
        if (me.chart.animate) {
            me.onAnimate(me.pointerSprite, {
                to: rotateConfig
            });
        } else {
            me.pointerSprite.setAttributes(rotateConfig, true);
        }
    },

    drawPointer: function () {
        var me = this,
            chart = me.chart,
            field = me.angleField || me.field || me.xField,
            pointerLength = me.radius * 0.8,
            pointerWidth = me.radius * 0.01,
            pointerNeedleLength = 10,
            axis = chart.axes.get(0),
            minimum = axis && axis.minimum || me.minimum || 0,
            maximum = axis && axis.maximum || me.maximum || 0,
            record = chart.getChartStore().getAt(0),
            value = me.value || record ? record.get(field) : 0,
            spriteConfig = {
                type: "path",
                path: [
                    'M', 0, pointerWidth / 2,
                    'h', -pointerLength + pointerNeedleLength,
                    'l', -pointerNeedleLength, -(pointerWidth / 2),
                    pointerNeedleLength, -(pointerWidth / 2),
                    'h', pointerLength - pointerNeedleLength,
                    'v', pointerWidth
                ],
                fill: "black",
                translate: {
                    x: me.centerX,
                    y: me.centerY
                }
            };

        value = me.reverse ? maximum - value : value;

        if (chart.resizing && me.pointerSprite) {
            me.pointerSprite.destroy();
            delete me.pointerSprite;
        }
        if (!me.pointerSprite) {
            me.pointerSprite = me.chart.surface.add(spriteConfig).redraw();

        } else {
            me.pointerSprite.setAttributes(spriteConfig, true);
        }
        me.movePointerToCurrectPostion(value, minimum, maximum);
    },

    drawGraduateFragment: function (segmentSpirteConfig, fragmentWidth, sprites) {
        var me = this,
            segment = segmentSpirteConfig.segment,
            orignalStartAngle = segment.startAngle,
            orignalEndAngle = segment.endAngle,
            totalAngle = orignalEndAngle - orignalStartAngle,
            count = totalAngle / fragmentWidth,
            i = 0,
            startAngle = orignalStartAngle,
            endAngle = startAngle + fragmentWidth,
            spritesCount = 0;

        sprites = sprites || [];
        spritesCount = sprites.length;
        segment.margin = 8;
        segmentSpirteConfig.group = me.group;

        if (startAngle % fragmentWidth) {
            endAngle = startAngle - startAngle % fragmentWidth;
        }

        while (startAngle < orignalEndAngle) {
            if (endAngle > orignalEndAngle) {
                endAngle = orignalEndAngle;
            }

            segment.startAngle = startAngle;
            segment.endAngle = endAngle;
            if (!sprites[i]) {
                sprites[i] = me.chart.surface.add(segmentSpirteConfig).redraw();
            } else {
                sprites[i].setAttributes(segmentSpirteConfig, true);
            }

            startAngle = endAngle;
            endAngle = startAngle + fragmentWidth;
            i += 1;
        }

        for (; i < spritesCount; i++) {
            sprites.pop().remove();
        }

        return sprites;
    },

    drawGraduateSegment: function (startAngle, endAngle, fill, sprites) {
        var me = this,
            spirteConfig = {
                type: "path",
                fill: fill,
                segment: {
                    startAngle: -180 + startAngle,
                    endAngle: -180 + endAngle,
                    rho: me.radius,
                    startRho: me.radius * 0.9,
                    endRho: (me.radius * 0.9) - (me.radius * 0.2)
                }
            };
        return me.drawGraduateFragment(spirteConfig, me.fragmentWidth, sprites);
    },

    drawGraduate: function () {
        var me = this,
            axis = me.chart.axes.get(0),
            minimum = axis && axis.minimum || me.minimum || 0,
            maximum = axis && axis.maximum || me.maximum || 0,
            graduate = me.graduate,
            warnDegree = graduate.warnDegree || maximum,
            dangerDegree = graduate.dangerDegree || maximum,
            reverse = dangerDegree < warnDegree,
            safeSAngle = reverse ? 180 * warnDegree / (maximum - minimum) : 0,
            safeEAngle = reverse ? 180 : 180 * warnDegree / (maximum - minimum),
            warnSAngle = reverse ? 180 * dangerDegree / (maximum - minimum) : 180 * warnDegree / (maximum - minimum),
            warnEAngle = reverse ? 180 * warnDegree / (maximum - minimum) : 180 * dangerDegree / (maximum - minimum),
            dangerSAngle = reverse ? 0 : 180 * dangerDegree / (maximum - minimum),
            dangerEAngle = reverse ? 180 * dangerDegree / (maximum - minimum) : 180;

        me.safeGraduates =
            me.drawGraduateSegment(safeSAngle, safeEAngle, me.safeGraduateColor, me.safeGraduates);

        me.warnGraduates =
            me.drawGraduateSegment(warnSAngle, warnEAngle, me.warnGraduateColor, me.warnGraduates);

        me.dangerGraduates =
            me.drawGraduateSegment(dangerSAngle, dangerEAngle, me.dangerGraduateColor, me.dangerGraduates);
    },

    getBackplaneStyleConfig: function () {
        var me = this,
            colorArrayStyle = me.colorArrayStyle,
            gradientId = me.seriesId + '-backplaneGradient';

        if (me.chart.theme !== 'Base') {
            colorArrayStyle = [colorArrayStyle[colorArrayStyle.length - 1], colorArrayStyle[0]];
        } else {
            colorArrayStyle = me.backplaneBaseColors;
        }
        if (!me.hasBackplaneGradient) {
            me.chart.surface.addGradient({
                id: gradientId,
                angle: 45,
                stops: {
                    0: {
                        color: colorArrayStyle[0]
                    },
                    100: {
                        color: colorArrayStyle[1]
                    }
                }
            });
            me.hasBackplaneGradient = true;
        }

        return {
            fill: 'url(#' + gradientId + ')',
            stroke: '#ccc',
            'stroke-width': 1
        };
    },

    drawBackplane: function () {
        var me = this,
            spriteConfig = {
                type: "path",
                segment: {
                    startAngle: -180,
                    endAngle: 0,
                    rho: me.radius,
                    startRho: 0,
                    endRho: me.radius
                }
            };

        spriteConfig = Ext.apply(spriteConfig, me.getBackplaneStyleConfig());

        if (!me.backplaneSprite) {
            me.backplaneSprite = me.chart.surface.add(spriteConfig).redraw();
        }
        else {
            me.backplaneSprite.setAttributes(spriteConfig, true);
        }
    },

    drawTextBox: function () {
        var me = this,
            chart = me.chart,
            chartBBox = chart.chartBBox,
            axis = chart.axes.get(0),
            field = me.angleField || me.field || me.xField,
            record = chart.getChartStore().getAt(0),
            value = me.value || record ? record.get(field) : 0,
            width = 100,
            height = 40,
            padding = 4,
            exceptTextHeight = chartBBox.height / 8,
            boxCongfig = {
                type: 'rect',
                width: width,
                height: height,
                'stroke-width': 1
            },
            textConfig = {
                type: "text",
                text: axis.label && axis.label.renderer ? axis.label.renderer(value) : value,
                font: Math.floor((exceptTextHeight * 10 - 30) / 11) + "px monospace"
            };


        if (chart.resizing && me.textBox) {
            me.textBox.destroy();
            delete me.textBox;
        }
        if (chart.resizing && me.valueText) {
            me.valueText.destroy();
            delete me.valueText;
        }
        if (!me.textBox) {
            me.textBox = me.chart.surface.add(boxCongfig);
            me.valueText = me.chart.surface.add(textConfig).redraw();
        }

        var boxWidth = me.valueText.getBBox().width + padding * 2;
        Ext.apply(boxCongfig, {
            width: boxWidth,
            translate: {
                x: me.centerX - boxWidth / 2,
                y: me.centerY - me.backplaneSprite.getBBox().height / 2.7 - height / 2
            }
        });

        Ext.apply(textConfig, {
            translate: {
                x: me.centerX - boxWidth / 2 + padding,
                y: me.centerY - me.backplaneSprite.getBBox().height / 2.7
            }
        });

        me.textBox.setAttributes(boxCongfig, true);
        me.valueText.setAttributes(textConfig, true);
    },

    drawSeries: function () {
        var me = this,
            chartBBox = me.chart.chartBBox,
            centerX = me.centerX = chartBBox.x + (chartBBox.width / 2),
            centerY = me.centerY = chartBBox.y + chartBBox.height;

        me.radius = Math.min(centerX - chartBBox.x, centerY - chartBBox.y);

        me.drawBackplane();
        me.drawTextBox();
        me.drawGraduate();
        me.drawPointer();
        me.drawPointerAxis();
    }
});