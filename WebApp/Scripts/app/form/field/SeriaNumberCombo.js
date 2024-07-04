/** 
 * 流水號下拉選單
 *
 * 使用範例：
 *
 *     @example
 *     Ext.create('Ext.Viewport',{
 *         items: [
 *             { 
 *                 xtype: 'serianumbercombo',
 *                 startNum: 1,
 *                 endNum: 99,
 *                 step: 1
 *             }
 *         ],
 *         renderTo: Ext.getBody()
 *     });
 */
Ext.define('MMIS.form.field.SeriaNumberCombo', {
    extend: 'Ext.form.field.ComboBox',
    alias: 'widget.serianumbercombo',

    /**
     * @cfg {Number} startNum (required)
     */
    startNum : 1,

    /**
     * @cfg {Number} endNum (required)
     */
    endNum: 99,

    /**
     * @cfg {Number} step
     */
    step: 1,

    valueField: 'value',

    initComponent: function () {
        var values = [],
            me = this,
            startNum = me.startNum,
            endNum = me.endNum,
            step = me.step,
            padSize = Math.max(startNum.toString().length, endNum.toString().length),
            i = startNum,
            addItem = function (i) {
                var item = Ext.String.leftPad(i.toString(), padSize, '0');
                values.push({ "text": item, "value": item });
            };

        if (startNum > endNum) {
            for (; i >= endNum; i += step) {
                addItem(i);
            }
        } else {
            for (; i <= endNum; i += step) {
                addItem(i);
            }
        }

        me.store = Ext.create('Ext.data.Store', {
            fields: ['text', 'value'],
            data: values
        });

        me.callParent(arguments);
    }
});