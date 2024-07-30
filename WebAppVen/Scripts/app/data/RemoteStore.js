/**
* A RemoteStore auto using ajax proxy
*/
Ext.define('WEBAPP.data.RemoteStore', {
    extend: 'Ext.data.Store',

    requires: ['WEBAPP.data.Util'],

    autoLoad: true,

    insertEmptyRow: false,

    constructor: function (config) {
        var me = this;
        config = config || {};

        if (config.proxy && config.queryParam) {
            config.listeners = {
                beforeload: function (store, options) {
                    Ext.apply(store.proxy.extraParams, config.queryParam);
                }
            };
        }

        var result = {};

        /*
        if (config.proxy)
            result = Ext.applyIf(config.proxy, this.getDefaultProxyConfig(config));
        else
            result = Ext.apply(config, { proxy: this.getDefaultProxyConfig(config) });
        */

        
        if (config.proxy)
            result = Ext.applyIf(config.proxy, this.getDefaultProxyConfig());
        else
            result = Ext.apply(config, { proxy: this.getDefaultProxyConfig() });
        

        //result = Ext.merge(config, { proxy: this.getDefaultProxyConfig() });

        Ext.apply(me, result);
        
        me.callParent(arguments);
        
        if (config.insertEmptyRow || me.insertEmptyRow) {
            me.on('load', function (store) {
                store.insert(0, { TEXT: '', VALUE: '' });
            });
        }
    },

    getDefaultProxyConfig: function () {
        return {
            type: 'ajax',
            actionMethods: { create: 'POST', read: 'POST', update: 'POST', destroy: 'POST' },
            timeout: 60000,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            },
            listeners: {
                exception: function (proxy, response, operation) {
                    WEBAPP.data.Util.ajaxErrorProcess(proxy, response, operation);
                }
            }
        };
    }
});
