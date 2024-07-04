/**
 * RemoteStore for combo
 */
Ext.define('MMIS.data.ComboStore', {
    extend: 'MMIS.data.RemoteStore',

    fields: ['TEXT', 'VALUE'],

    autoLoad: true,

    remoteSort: false,

    insertEmptyRow: true,

    constructor: function (config) {
        var me = this;

        config = config || {};

        config.rootTable = config.rootTable || "ds.T";

        me.callParent([config]);

        if (me.insertEmptyRow) {
            me.on('load', function (store) {
                store.insert(0, { TEXT: '', VALUE: '' });
            });
        }
    }
});