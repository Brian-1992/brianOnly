/** 
 * 項目選擇欄位
 *
 * 使用範例：
 *
 *     @example
 *     Ext.create('Ext.Viewport',{
 *         items: [
 *             { 
 *                 xtype: 'triggeritemselector',
 *                 taskFlow: 'TraMMInvDeptComboGet',
 *                 windowWidth: 500,
 *                 windowHeight: 220
 *             }
 *         ],
 *         renderTo: Ext.getBody()
 *     });
 */
Ext.define('MMIS.form.field.ItemSelector', {
    extend: 'Ext.form.field.Trigger',
    alias: 'widget.triggeritemselector',

    requires: [
        'MMIS.Utility',
        'MMIS.data.RemoteStore',
        'Ext.ux.form.ItemSelector'
    ],

    /**
     * @cfg {String} title
     */
    title: '請選擇',

    /**
     * @cfg {String} taskFlow
     */

    /**
     * @cfg {String} rootTable
     */
    rootTable: 'ds.T',

    /**
     * @cfg {String} displayField
     */
    displayField: 'TEXT',

    /**
     * @cfg {String} valueField
     */
    valueField: 'VALUE',

    /**
     * @cfg {Ext.data.Store} windowWidth
     */
    windowWidth: 500,

    /**
     * @cfg {Ext.data.Store} windowHeight
     */
    windowHeight: 400, 

    /**
     * @cfg {Ext.data.Store} store
     */

    /**
     * @property {Ext.ux.form.ItemSelector} itemSelector
     * @private
     */

    /**
     * @property {Ext.window.Window} window
     * @private
     */

    triggerCls: Ext.baseCSSPrefix + 'form-search-trigger',

    initComponent: function () {

        MMIS.Utility.loadCss('/Scripts/extjs/ux/css/ItemSelector.css');

        var me = this,

            itemSelector = Ext.create("Ext.ux.form.ItemSelector", {
                anchor: '100%',
                store: me.store || Ext.create('MMIS.data.RemoteStore', {
                    taskFlow: me.taskFlow,
                    rootTable: me.rootTable,
                    fields: [me.displayField, me.valueField],
                    autoLoad: true,
                    remoteSort: false
                }),
                displayField: 'TEXT',
                valueField: 'VALUE',
                buttons: ['add', 'remove'],
                maxSelections: me.maxSelections,
                maxSelectionsText: '最多選{0}個項目',
                msgTarget: 'side'
            }),

            window = Ext.widget('window', {
                title: me.title,
                height: me.windowHeight,
                width: me.windowWidth,
                layout: 'fit',
                items: itemSelector,
                closeAction: 'hide',
                buttons: [{
                    text: '完成',
                    handler: function () {
                        var itemSelector = me.itemSelector;
                        if (itemSelector.isValid()) {
                            me.setValue(itemSelector.getValue());
                            me.window.close();
                        }
                    }
                }]
            });


        Ext.apply(me, {
            itemSelector: itemSelector,
            window: window
        });

        me.callParent(arguments);
    },

    onTriggerClick: function () {
        var me = this;
        me.itemSelector.setValue(me.getValue());
        me.window.show();
    }
});