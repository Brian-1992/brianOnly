Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

//Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var T11Set = '';
    var T1Name = "廠商批號、效期維護";

    var T1Rec = 0;
    var T1LastRec = null;
    var T11Rec = 0;
    var T11LastRec = null;
    var inidc;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var st_matclass = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BE0004/GetMatclassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, records, successful) {
                if (store.data.length > 0) {
                    T1Query.getForm().findField('P0').setValue(store.data.items[0].data.VALUE);
                }
            }
        },
        autoLoad: true
    });

    var statusStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [    
            { "VALUE": "", "TEXT": "全部" },
            { "VALUE": "Y", "TEXT": "已結案" },
            { "VALUE": "N", "TEXT": "未結案" }
        ]
    });
    var statusStore1 = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { "VALUE": "Y", "TEXT": "已結案" },
            { "VALUE": "N", "TEXT": "未結案" }
        ]
    });
    var sourceStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { "VALUE": "U", "TEXT": "自行輸入" },
            { "VALUE": "V", "TEXT": "廠商輸入" }
        ]
    });
    var T1QuryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P3',
        name: 'P3',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        labelWidth: 60,
        width: 200,
        limit: 200, //限制一次最多顯示10筆
        queryUrl: '/api/AA0061/GetMMCODECombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                mat_class: T1Query.getForm().findField('P0').getValue()  //P0:預設是動態MMCODE
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        },
    });

    var MMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'MMCODE',
        name: 'MMCODE',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        labelWidth: 60,
        width: 200,
        limit: 200, //限制一次最多顯示10筆
        queryUrl: '/api/AA0061/GetMMCODECombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                mat_class: T1Query.getForm().findField('P0').getValue()  //P0:預設是動態MMCODE
            };
        },
        listeners: {
            select: function (c, r, i, e) {
               
                //選取下拉項目時，顯示回傳值
                T1Form.getForm().findField('MMNAME_C').setValue(r.get('MMNAME_C'));
                T1Form.getForm().findField('MMNAME_E').setValue(r.get('MMNAME_E'));
            }
        },
    });

    function getDefaultValue(isEndDate) {
        var yyyy = 0;
        var m = 0;
        if (isEndDate == "P2") {
            yyyy = new Date().getFullYear() - 1906;
            m = new Date().getMonth() + 1;
        } else if (isEndDate == "P1") {    //減6個月
            var date = new Date();
            date.setMonth(date.getMonth() - 11);

            yyyy = date.getFullYear() - 1911;
            m = date.getMonth();
            if (m == 0) {   //因為從目前六月算起，的前六月是12月，但它跑出來是0
                yyyy = yyyy - 1;
                m = 12;
            }
        }

        var d = 0;
        d = new Date().getDate();

        var mm = m > 10 ? m.toString() : "0" + m.toString();
        var dd = d > 10 ? d.toString() : "0" + d.toString();

        return yyyy.toString() + mm + dd;

    }


    var mLabelWidth = 60;
    var mWidth = 180;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'combo',
                    fieldLabel: ' 物料分類',
                    name: 'P0',
                    id: 'P0',
                    store: st_matclass,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    //allowBlank: false, // 欄位為必填
                    fieldCls: 'required',
                    labelAlign: 'right',
                    labelWidth: mLabelWidth,
                    width: mWidth,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'datefield',
                    fieldLabel: '效期日期',
                    name: 'P1',
                    id: 'P1', dateRange: { end: 'P2' },
                    value: getDefaultValue("P1"),
                    vtype: 'dateRange',
                    //dateRange: { end: 'P2' },
                    padding: '0 4 0 4',
                    //allowBlank: false, // 欄位為必填
                    //fieldCls: 'required',
                    labelAlign: 'right',
                    labelWidth: mLabelWidth,
                    width: mWidth
                    //value: getFirstday()
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    labelWidth: '10px',
                    name: 'P2',
                    id: 'P2',
                    labelSeparator: '',
                    vtype: 'dateRange',
                    //dateRange: { begin: 'P1' },
                    //dateRange: { end: 'P1' },
                    value: getDefaultValue("P2"),
                    padding: '0 4 0 4',
                    //allowBlank: false, // 欄位為必填
                    //fieldCls: 'required',
                    labelAlign: 'right',
                    labelWidth: 30,
                    width: 170
                    // value: getToday()
                }
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            items: [
                {

                    xtype: 'textfield',
                    fieldLabel: '批號',
                    name: 'P4',
                    id: 'P4',
                    enforceMaxLength: true,
                    maxLength: 10,
                    padding: '0 4 0 4'
                },
                T1QuryMMCode,
                {
                    xtype: 'combo',
                    fieldLabel: '狀態',
                    name: 'P5',
                    id: 'P5',
                    store: statusStore,
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    //allowBlank: false, // 欄位為必填

                    labelAlign: 'right',
                    labelWidth: mLabelWidth,
                    width: 170,
                    padding: '0 4 0 4'

                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        if ((T1Query.getForm().findField('P0').getValue() == "" || T1Query.getForm().findField('P0').getValue() == null)) {

                            Ext.Msg.alert('訊息', '物料分類為必填');
                            return;
                        }
                        //if (((T1Query.getForm().findField('P1').getValue() == "" || T1Query.getForm().findField('P1').getValue() == null))
                        //    || ((T1Query.getForm().findField('P2').getValue() == "" || T1Query.getForm().findField('P2').getValue() == null))) {
                        //    Ext.Msg.alert('訊息', '效期日期為必填');
                        //    return;
                        //}
                        Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                        //getINID(parent.parent.userId);
                        T1Load();
                        msglabel('訊息區:');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus();
                        msglabel('訊息區:');
                    }
                }
            ]
        }]
    });
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [' MAT_CLASS', 'MMCODE', 'MMNAME_C ', 'MMNAME_E', 'EXP_DATE', 'LOT_NO', 'UPDATE_USER', 'UPDATE_IP', 'QTY', 'MEMO', 'SOURCE', 'PO_NO', 'SEQ']

    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        //pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'MAT_CLASS', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().getValues()['P0'],
                    p1: T1Query.getForm().getValues()['P1'],
                    p2: T1Query.getForm().getValues()['P2'],
                    p3: T1Query.getForm().getValues()['P3'],
                    p4: T1Query.getForm().getValues()['P4'],
                    p5: T1Query.getForm().getValues()['P5'],
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BE0004/MasterAll',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });
    function T1Load() {
        T1Tool.moveFirst();
    }




    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增', handler: function () {
                    T1Set = '/api/BE0004/MasterCreate';
                    msglabel('訊息區:');
                    setFormT1('I', '新增');
                }
            }, {
                itemId: 't1edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/BE0004/MasterUpdate';
                    msglabel('訊息區:');
                    setFormT1("U", '修改');
                }
            }
        ]
    });
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        // viewport.down('#t11Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();

        if (x === "I") {
            isNew = true;
            //alert(x);
            var r = Ext.create('T1Model');
            T1Form.loadRecord(r);
            f.findField('MMCODE').setReadOnly(false);
            f.findField('MMNAME_C').setReadOnly(false);
            f.findField('MMNAME_E').setReadOnly(false);
            f.findField('EXP_DATE').setReadOnly(false);
            f.findField('MEMO').setReadOnly(false);
            f.findField('LOT_NO').setReadOnly(false);
            f.findField('PO_NO').setReadOnly(false);
            f.findField('QTY').setReadOnly(false);
            f.findField('EXP_DATE').setValue('');
            f.findField('LOT_NO').setValue('');
            f.findField('QTY').setValue('');
            f.findField('SOURCE').setValue('U');
        }
        f.findField('x').setValue(x);
        if (f.findField('SOURCE').getValue() == 'V')
        {
            f.findField('MMCODE').setReadOnly(true);
            f.findField('LOT_NO').setReadOnly(true);
            f.findField('EXP_DATE').setReadOnly(true);
            f.findField('STATUS').setReadOnly(false);
            f.findField('PO_NO').setReadOnly(true);
            f.findField('QTY').setReadOnly(true);
            f.findField('MEMO').setReadOnly(false);
        }
        else {
            f.findField('MMCODE').setReadOnly(false);
            f.findField('LOT_NO').setReadOnly(false);
            f.findField('EXP_DATE').setReadOnly(false);
            f.findField('STATUS').setReadOnly(false);
            f.findField('PO_NO').setReadOnly(false);
            f.findField('QTY').setReadOnly(false);
            f.findField('MEMO').setReadOnly(false);
        }
        f.findField('SOURCE').setReadOnly(true);
        T1Form.down('#t1cancel').setVisible(true);
        T1Form.down('#t1submit').setVisible(true);
    }


    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        }, {

            text: "物料分類",
            dataIndex: 'MAT_CLASS',
            style: 'text-align:left',
            align: 'left',
            width: 140
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            style: 'text-align:left',
            align: 'left',
            width: 80
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            style: 'text-align:left',
            align: 'left',
            width: 100
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            style: 'text-align:left',
            align: 'left',
            width: 200
        }, {
            text: "批號",
            dataIndex: 'LOT_NO',
            style: 'text-align:left',
            align: 'left',
            width: 200
        }, {
            text: "效期",
            dataIndex: 'EXP_DATE',
            style: 'text-align:left',
            align: 'left',
            width: 100
        }, {
            text: "數量",
            dataIndex: 'QTY',
            style: 'text-align:left',
            align: 'left',
            width: 100
        }, {
            text: "來源",
            dataIndex: 'SOURCE',
            style: 'text-align:left',
            align: 'left',
            width: 100
        }, {
            text: "狀態",
            dataIndex: 'STATUS',
            style: 'text-align:left',
            align: 'left',
            width: 100
        }, {
            text: "訂單編號",
            dataIndex: 'PO_NO',
            style: 'text-align:left',
            align: 'left',
            width: 100
        }, {
            text: "備註",
            dataIndex: 'MEMO',
            style: 'text-align:left',
            align: 'left',
            width: 100
        }, {
            name: 'SEQ',
            xtype: 'hidden'
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T1Form.hidden === true) {
                        T1Form.setVisible(true);
                        //T11Form.setVisible(false);
                    }
                }
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                Ext.ComponentQuery.query('panel[itemId=form]')[0].expand();
                //ComboData();
                setFormT1a();
                //T11Load();
            }
        }
    });
    function setFormT1a() {
        T1Grid.down('#t1edit').setDisabled(T1Rec == 0);
        if (T1LastRec) {
            isNew = false;
            console.log(T1LastRec);
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('MMCODE').setReadOnly(true);
            f.findField('MMNAME_C').setReadOnly(true);
            f.findField('MMNAME_E').setReadOnly(true);
            f.findField('EXP_DATE').setReadOnly(true);
            f.findField('LOT_NO').setReadOnly(true);
            f.findField('PO_NO').setReadOnly(true);
            f.findField('QTY').setReadOnly(true);

            T1Grid.down('#t1edit').setDisabled(false);
        }
        else {
            T1Form.getForm().reset();

        }

    }



    var T1Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden',
            width: 220
        },
            MMCode,
        {
            xtype: 'textfield',
            fieldLabel: '中文品名',
            readOnly: true,
            name: 'MMNAME_C'
        }, {
            xtype: 'textfield',
            fieldLabel: '英文品名',
            readOnly: true,
  
            name: 'MMNAME_E'
        }, {
            xtype: 'textfield',
            fieldLabel: '批號',
            name: 'LOT_NO',
            allowBlank: false,
            fieldCls: 'required'
        }, {
            xtype: 'datefield',
            fieldLabel: '效期',
            readOnly: true,
            name: 'EXP_DATE',
            allowBlank: false,
            fieldCls: 'required'
        }, {
            xtype: 'textfield',
            fieldLabel: '數量',
            name: 'QTY',
            allowBlank: false,
            fieldCls: 'required'
        }, {
            xtype: 'combo',
            fieldLabel: '狀態',
            store: statusStore1,
            queryMode: 'local',
            displayField: 'TEXT',
            valueField: 'VALUE',
            editable: false, 
            name: 'STATUS',
            allowBlank: false,
            fieldCls: 'required'
        }, {
            xtype: 'textfield',
            fieldLabel: '訂單編號',
            allowBlank: false,
            name: 'PO_NO',
            fieldCls: 'required'
        }, {
            xtype: 'textareafield',
            fieldLabel: '備註',
            name: 'MEMO',
            enforceMaxLength: true,
            maxLength: 300,
            height: 200,
            readOnly: true
        }, {
            xtype: 'combo',
            fieldLabel: '來源',
            store: sourceStore,
            queryMode: 'local',
            displayField: 'TEXT',
            valueField: 'VALUE',
            name: 'SOURCE'
            //fieldCls: 'required'
            //xtype: 'textfield',
            //fieldLabel: '來源',
            //readOnly: true,
            //name: 'SOURCE'
        }, {
            xtype: 'displayfield',
            fieldLabel: '',
            labelWidth: 10,
            Width: 320,
            labelSeparator: '',
            border: false,
            value: '<font color=blue>[來源]為"廠商輸入"時，只能修改[狀態]及[備註]</font>'
        }, { 
            xtype: 'hidden',
            name: 'SEQ'
        }],
        buttons: [{
            itemId: 't1submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) {
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();

                        }
                    }
                    );
                }
                else {
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
                    msglabel('訊息區:輸入資料格式有誤');
                }
            }
        }, {
            itemId: 't1cancel', text: '取消', hidden: true, handler: T1Cleanup
        }]
    });
    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    console.log(r)
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            //var v = action.result.etts[0];
                            //T1Query.getForm().findField('P0').setValue(v.mat_class);
                            T1Load();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            T1Load();
                            msglabel('訊息區:資料修改成功');
                            break;
                        case "D":
                            T1Store.remove(r);
                            r.commit();
                            msglabel('訊息區:資料刪除成功');
                            break;

                    }
                    T1Cleanup();
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('失敗', action.result.msg);
                            break;
                    }
                }
            });
        }
    }
    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        //  viewport.down('#t11Grid').unmask();
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if (fc.xtype == "displayfield" || fc.xtype == "textfield" || fc.xtype == "combo") {
                fc.setReadOnly(true);
            } else if (fc.xtype == "datefield") {
                fc.readOnly = true;
            }
        });
        T1Form.down('#t1cancel').hide();
        T1Form.down('#t1submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
        setFormT1a();
    }

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [
            {
                itemId: 'tGrid',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [{
                    //  xtype:'container',
                    region: 'center',
                    layout: {
                        type: 'border',
                        padding: 0
                    },
                    collapsible: false,
                    title: '',
                    split: true,
                    width: '80%',
                    flex: 1,
                    minWidth: 50,
                    minHeight: 140,
                    items: [
                        {
                            itemId: 't1Grid',
                            region: 'center',
                            layout: 'fit',
                            collapsible: false,
                            title: '',
                            border: false,
                            items: [T1Grid]

                        }
                    ]
                }]
            },
            {
                itemId: 'form',
                region: 'east',
                collapsible: true,
                floatable: true,
                width: 350,
                title: '瀏覽',
                border: false,
                collapsed: true,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [T1Form]
            }
        ]
    });
    T1Query.getForm().findField('P0').focus();
});
