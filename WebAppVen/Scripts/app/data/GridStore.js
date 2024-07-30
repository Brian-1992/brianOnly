/**
 * RemoteStore for grid
 */
Ext.define('MMIS.data.GridStore', {
    extend: 'MMIS.data.RemoteStore',

    rootTable: 'ds.T1',
    totalProperty: 'ds.T1C[0].RC',

    remoteSort: true,

    constructor: function (config) {
        this.callParent(arguments);
    },

    getDefaultConfig: function (config) {

        var myConfig = {
            pageSize: 20,
            rootTable: 'ds.T1',
            totalProperty: 'ds.T1C[0].RC'
        };

        Ext.applyIf(config, myConfig);

        var parentConfig = this.callParent(arguments);
        Ext.applyIf(myConfig, parentConfig);

        return myConfig;
    }
});