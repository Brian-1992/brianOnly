/**
 * 包含自動填寫欄位標籤功能
 *
 *     @example
 *     Ext.define('FormTestModel', {
 *         extend: 'Ext.data.Model',
 *         fields: [
 *             { name: 'name1', type: 'string', displayName: '名稱1' },
 *             { name: 'name2', type: 'string', displayName: '名稱2' }
 *         ]
 *     });
 *
 *     var myform = Ext.create('MMIS.form.Form', {
 *         model: 'FormTestModel',
 *         items: [
 *             { name: 'name1' },
 *             { name: 'name2', required: true }
 *         ],
 *         renderTo: Ext.getBody()
 *     });
 *
 * 當items config 給予required: true 的選項時等同給予以下選項
 * 
 *     {
 *         fieldCls: "required";
 *         allowBlank: false;
 *         allowOnlyWhitespace: false;
 *     }
 * 
 * items裡的內容也可以只填寫欄位名稱，例如：
 *
 *     var myform = Ext.create('MMIS.form.Form', {
 *         model: 'FormTestModel',
 *         items: ['name1', 'name2']
 *     });
 *
 */
Ext.define('MMIS.form.Form', {
    extend: 'Ext.form.Panel',
    alias: 'widget.mmisform',
    requires: ['MMIS.data.ModelHalper'],

    /**
     * @cfg {String} model
     */

    bodyPadding: 3,

    defaultType: 'textfield',

    initComponent: function () {
        var me = this;

        me.beforeInitComponent();

        me.callParent(arguments);

        me.afterInitComponent();
    },

    beforeInitComponent: function () {
        var me = this;

        Ext.apply(me, {
            items: me.model ? me.transformItems(me.items) : me.items,
            fieldDefaults: Ext.apply({
                msgTarget: 'side',
                labelAlign: "right"
            }, me.fieldDefaults)
        });
    },

    afterInitComponent: Ext.emptyFn,

    transformItems: function (items) {
       
        var me = this,
            modelHalper = MMIS.data.ModelHalper,
            transformStringToObject = function (name) {
                return { name: name };
            },
            fillFieldLabel = function fillFieldLabel(item) {
                item.fieldLabel = modelHalper.getDisplayName(me.model, item.name);
            },
            fillRequiredFieldConfig = function fillRequiredFieldConfig(item) {
                item.fieldCls = "required";
                item.allowBlank = false;
                item.allowOnlyWhitespace = false;
            };

        Ext.each(items, function (item, index) {

            if (item.items) {
                me.transformItems(item.items);
            }

            if (Ext.typeOf(item) == "string") {
                item = transformStringToObject(item);
                items[index] = item;
            }
           
            if (!item.fieldLabel && item.name) {
                fillFieldLabel(item);
            }

            if (item.required) {
                fillRequiredFieldConfig(item);
            }
        });

        return items;
    }
});