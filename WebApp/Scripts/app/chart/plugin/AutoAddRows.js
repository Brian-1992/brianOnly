Ext.define('MMIS.chart.plugin.AutoAddRows', {
    extend: 'Ext.AbstractPlugin',
    alias: 'plugin.autoaddrows',

    /**
     * @cfg {Number} minRowNum
     * 最小列數
     */
    minRowNum: 5,

    init: function (chart) {
        var me = this,
            series = chart.series.get(0);

        me.chart = chart;

        if (series && series.type === 'column') {
            me.enableAutoAdd();
        }
    },

    enableAutoAdd: function () {
        var me = this,
            oStore = me.oStore = me.chart.store,
            count,
            newStore;

        me.chart.store = Ext.create('Ext.data.Store', {
            model: oStore.model
        });

        me.updateChartStore();

        oStore.on('load', function () {
            me.updateChartStore();
        });
    },

    updateChartStore: function () {
        var me = this,
            minRowNum = me.minRowNum,
            oStore = me.oStore,
            count = oStore.count(),
            rs = [], i;

        for (i = count; i < minRowNum; i++) {
            rs.push(oStore.model.create());
        }
        
        me.chart.store.loadData(oStore.getRange().concat(rs));
    }
});