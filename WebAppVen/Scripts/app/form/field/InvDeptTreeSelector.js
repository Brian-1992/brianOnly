Ext.define('MMIS.form.field.InvDeptTreeSelector', {
    extend: 'Ext.form.field.Trigger',
    alias: 'widget.invdepttreeselector',

    /**
     * @cfg {String} title
     */
    title: '請選擇存料單位',

    /**
     * @cfg {Ext.data.Store} windowWidth
     */
    windowWidth: 350,

    /**
     * @cfg {Ext.data.Store} windowHeight
     */
    windowHeight: 400,

    triggerCls: Ext.baseCSSPrefix + 'form-search-trigger',

    initComponent: function () {
        var me = this,
            treeStore = Ext.create('MMIS.data.TreeStore', {
                fields: [
                    { name: 'id', type: 'string' },
                    { name: 'text', type: 'string' },
                    { name: 'checked', type: 'bool' }
                ],
                proxy: {
                    type: 'ajax',
                    url: '../../../api/DSTree/GetInvDeptNodes'
                }
            }),
            treePanel = Ext.create('Ext.tree.Panel', {
                store: treeStore,
                rootVisible: false,
                useArrows: true
            }),
            window = Ext.widget('window', {
                title: me.title,
                height: me.windowHeight,
                width: me.windowWidth,
                layout: 'fit',
                items: [treePanel],
                closeAction: 'hide',
                buttons: [{
                    text: '完成',
                    handler: function () {
                        var records = treePanel.getView().getChecked(),
                            ids = [];
                        Ext.Array.each(records, function (rec) {
                            ids.push(rec.get('id'));
                        });
                        me.setValue(ids.join(','));
                        me.window.close();
                    }
                }]
            });

        Ext.apply(me, {
            treePanel: treePanel,
            window: window
        });

        me.callParent(arguments);
    },

    onTriggerClick: function () {
        var me = this;
        me.window.show();
    }

});