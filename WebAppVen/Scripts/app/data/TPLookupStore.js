Ext.define('MMIS.data.TPLookupStore', {
    extend: 'MMIS.data.ComboStore',

    lookupTableName: '',
    lookupColName: '',
    textType: 0, //0:TEXT(VALUE) 1:TEXT

    fields: ['TEXT', 'VALUE'],
    autoLoad: true,
    remoteSort: false,

    constructor: function (config) {        
        config.taskFlow = 'TraTP10001GetLOOKUP';
        this.callParent([config]);
    },

    listeners: {
        beforeload: function (store, options) {
            var np = {
                p0: this.lookupTableName,
                p1: this.lookupColName,
                p2: this.textType
            };
            Ext.apply(store.proxy.extraParams, np);
        }
    }
});