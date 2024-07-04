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
    var T1Name = "不同大小包裝量維護";

    var T1Rec = 0;
    var T1LastRec = null;
    var T11Rec = 0;
    var T11LastRec = null;
    var inidc;

    var windowHeight = $(window).height();
    var windowWidth = $(window).width();

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var wh_NoCombo = Ext.create('WEBAPP.form.WH_NoCombo', {
        name: 'P0',
        id: 'P0',
        fieldLabel: '庫別代碼',
        fieldCls: 'required',
        allowBlank: false,
        width: 200,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AB0048/GetWH_NOComboNotOne',

        //查詢完會回傳的欄位
        extraFields: ['WH_NO', 'WH_NAME'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {

        },
        listeners: {
            select: function (c, r, i, e) {

                WH_NAME = r.get('WH_NAME');
                Ext.getCmp('P1').setDisabled(false);
                Ext.getCmp('P2').setDisabled(false);
                T2Query.getForm().findField('wh_no').setValue(r.get('WH_NO') + ' ' + WH_NAME);
            },
            blur: function (field, eOpts) {
                
                chkWhNo();
            }
        }
    });
    var T1QuryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P1',
        name: 'P1',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        width: 200,
        disabled: true,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0048/GetMMCODECombo',//指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1Query.getForm().findField('P0').getValue()  //P0:預設是動態MMCODE
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });

    var T1QuryMMCode2 = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P2',
        name: 'P2',
        fieldLabel: '至',
        labelAlign: 'right',
        labelWidth: 7,
        width: 147,
        disabled: true,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0048/GetMMCODECombo',//指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1Query.getForm().findField('P0').getValue()  //P0:預設是動態MMCODE
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });
    var wh_No = Ext.create('WEBAPP.form.WH_NoCombo', {
        name: 'WH_NO',
        id: 'WH_NO',
        fieldLabel: '庫別代碼',
        allowBlank: false,
        fieldCls: 'required',
        width: 200,
        //限制一次最多顯示10筆
        limit: 10,
        //指定查詢的Controller路徑
        queryUrl: '/api/AB0048/GetWH_NOCombo',
        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],
        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {

        },
        listeners: {
            select: function (c, r, i, e) {

                WH_NAME = r.get('WH_NAME');

            }
        }
    });
    var MMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'MMCODE',
        name: 'MMCODE',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        fieldCls: 'required',
        allowBlank: false,
        labelWidth: 60,
        width: 200,
        limit: 10, //限制一次最多顯示10筆
        //queryUrl: '/api/AA0066/GetMMCODECombo',
        queryUrl: '/api/AB0048/GetMmcodeCombo',//指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1Form.getForm().findField('WH_NO').getValue()  //P0:預設是動態MMCODE
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                var f = T1Form.getForm();
                if (r.get('MMCODE') !== '') {
                    f.findField("MMCODE").setValue(r.get('MMCODE'));
                    //f.findField("MMNAME_C").setValue(r.get('MMNAME_C'));
                    f.findField("MMNAME_E").setValue(r.get('MMNAME_E'));
                    //f.findField("BASE_UNIT").setValue(r.get('BASE_UNIT'));
                }
            }
        },
    });


    var UnitCombo = Ext.create('WEBAPP.form.UnitCombo', {
        name: 'PACK_UNIT',
        id: 'PACK_UNIT',
        fieldLabel: '申請包裝單位',
        fieldCls: 'required',
        allowBlank: false,
        labelAlign: 'right',
        msgTarget: 'side',
        labelWidth: 60,
        width: 300,
        //width: 150,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AB0048/GetUnitCombo',

        //查詢完會回傳的欄位
        extraFields: ['PACK_QTY'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1Form.getForm().findField('MMCODE').getValue(),
               // p2: kind//P0:預設是動態MMCODE
                // T1QueryForm.getForm().findField('P0').getValue(),
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                var f = T1Form.getForm();
                if (r.get('PACK_UNIT') !== '') {
                    f.findField("PACK_UNIT").setValue(r.get('PACK_UNIT'));
                    f.findField("PACK_QTY").setValue(r.get('PACK_QTY'));
                }
            }
        }
    });

    var CtdmdComboGet = '/api/AB0048/GetCtdmdCombo';
    var CtdmdQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    function setComboData() {
        Ext.Ajax.request({
            url: CtdmdComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        CtdmdQueryStore.add({ VALUE: '', COMBITEM: '全部' });
                        for (var i = 0; i < tb_data.length; i++) {
                            CtdmdQueryStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
                T1Query.getForm().findField('P3').setValue('0');
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    function chkWhNo() {
        if (T1Query.getForm().findField('P0').getValue() == '') {
            return;
        }

        var wh_no = T1Query.getForm().findField('P0').getValue();

        Ext.Ajax.request({
            url: '/api/AA0048/CheckWhid',
            params: {
                wh_no: T1Query.getForm().findField('P0').getValue()
            },
            method: reqVal_p,
            success: function (response) {
                
                var data = Ext.decode(response.responseText);
                if (data.success) {

                } else {
                    Ext.getCmp('P1').setDisabled(true);
                    Ext.getCmp('P2').setDisabled(true);
                    T1Query.getForm().findField('P0').setValue('');
                    T1Query.getForm().findField('P1').setValue('');
                    T1Query.getForm().findField('P2').setValue('');
                    Ext.Msg.alert('訊息', data.msg);
                    return;
                }
            },
            failure: function (response, options) {

            }
        });
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
                wh_NoCombo,
                T1QuryMMCode,
                T1QuryMMCode2,
                {
                    xtype: 'combo',
                    fieldLabel: '停用碼',
                    name: 'P3',
                    id: 'P3',
                    store: CtdmdQueryStore,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                },
                {
                    xtype: 'button',
                    text: '顯示本庫房需修正項目',
                    handler: function () {
                        var p0 = T1Query.getForm().findField('P0').getValue() == null ? '' : T1Query.getForm().findField('P0').getValue();
                        if (p0 == "") {
                            T1Query.getForm().findField('P0').focus();
                            Ext.Msg.alert('提醒', '庫別代碼必選');
                            return;
                        }
                        T2Load(true);
                        differWidow.show();
                        msglabel('訊息區:');
                    }
                },
                //{
                //    fieldLabel: '僅顯示應修正品項',
                //    name: 'P4',
                //    xtype: 'checkboxfield',
                //    labelWidth: 120, width: 150,
                //    labelAlign: 'right',
                //},
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        var p0 = T1Query.getForm().findField('P0').getValue() == null ? '' : T1Query.getForm().findField('P0').getValue();
                        if (p0 == "") {
                            T1Query.getForm().findField('P0').focus();
                            Ext.Msg.alert('提醒', '庫別代碼必選');
                        }
                        else {
                            Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                            T1Load(true);
                        }
                        msglabel('訊息區:');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus();
                        Ext.getCmp('P1').setDisabled(true);
                        Ext.getCmp('P2').setDisabled(true);
                        T1Query.getForm().findField('P3').setValue('0');
                        msglabel('訊息區:');
                    }
                }
            ]
        }]
    });
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['WH_NO', 'MMCODE', 'MMNAME_E', 'PACK_UNIT', 'PACK_QTY', 'CREATE_TIME', 'CREATE_USER', 'UPDATE_TIME', 'UPDATE_USER', 'UPDATE_IP', 'INID', 'CTDMDCCODE','CTDMDCCODE_N']//ASKING_PERSON', 'RESPONDER', 'ASKING_DATE', 'CONTENT1', 'CHG_DATE', 'content', 'RESPONSE', 'RESPONSE_DATE', 'STATUS']//, 'Plant', 'PR_Create_By', 'RequestUnit', 'PR_DocType', 'Buyer', 'Status'],

    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        //pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().getValues()['P0'],
                    p1: T1Query.getForm().getValues()['P1'],
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0048/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            },
            timeout:90000
        }
    });
    function T1Load(moveFirst) {
        if (moveFirst) {
            T1Tool.moveFirst();

        } else {
            T1Store.load({
                params: {
                    start: 0
                }
            });
        }
    }




    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增', handler: function () {
                    T1Set = '/api/AB0048/Create';
                    msglabel('訊息區:');
                    setFormT1('I', '新增');
                }
            }, {
                itemId: 't1edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/AB0048/Update';
                    msglabel('訊息區:');
                    setFormT1("U", '修改');
                }
            }, {
                itemId: 't1delete', text: '刪除', disabled: true,
                handler: function () {
                    msglabel('訊息區:');
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/AB0048/Delete';
                            T1Form.getForm().findField('x').setValue('D');
                            T1Submit();
                        }
                    }
                    );
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
            f.reset();
            f.findField('MMCODE').setReadOnly(false);
            f.findField('WH_NO').setReadOnly(false);
            f.findField('PACK_UNIT').setReadOnly(false);
            f.findField('PACK_TIMES').setReadOnly(false);
        }
        else {
            //u = f.findField('INID');
        }
        f.findField('x').setValue(x);
        //f.findField('MMCODE').setReadOnly(false);
        //f.findField('WH_NO').setReadOnly(false);
        f.findField('PACK_UNIT').setReadOnly(false);
        f.findField('PACK_TIMES').setReadOnly(false);
        T1Form.down('#t1cancel').setVisible(true);
        T1Form.down('#t1submit').setVisible(true);
        // u.focus();

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
            text: "庫別代碼",
            dataIndex: 'WH_NO',
            style: 'text-align:left',
            align: 'left',
            width: 140,
            renderer: function (val, meta, record) {
                
                if (record.data.DIFFER == 'Y') {
                    return '<span style="color:red">' + val + '</span>';
                } else {
                    return val;
                }
                
            },
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            style: 'text-align:left',
            align: 'left',
            width: 140,
            renderer: function (val, meta, record) {
                
                if (record.data.DIFFER == 'Y') {
                    return '<span style="color:red">' + val + '</span>';
                } else {
                    return val;
                }

            },
        }, {
            text: "品名",
            dataIndex: 'MMNAME_E',
            style: 'text-align:left',
            align: 'left',
            width: 250,
            renderer: function (val, meta, record) {
                
                if (record.data.DIFFER == 'Y') {
                    return '<span style="color:red">' + val + '</span>';
                } else {
                    return val;
                }

            },
        }, {
            text: "申請包裝單位",
            dataIndex: 'PACK_UNIT',
            style: 'text-align:left',
            align: 'left',
            width: 100,
            renderer: function (val, meta, record) {
                
                if (record.data.DIFFER == 'Y') {
                    return '<span style="color:red">' + val + '</span>';
                } else {
                    return val;
                }

            },
        }, {
            text: "申請包裝轉換量",
            dataIndex: 'PACK_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 100,
            renderer: function (val, meta, record) {
                
                if (record.data.DIFFER == 'Y') {
                    return '<span style="color:red">' + val + '</span>';
                } else {
                    return val;
                }

            },
        }, {
                text: "包裝申領量倍數",
            dataIndex: 'PACK_TIMES',
            style: 'text-align:left',
            align: 'right',
            width: 120,
            renderer: function (val, meta, record) {
                
                if (record.data.DIFFER == 'Y') {
                    return '<span style="color:red">' + val + '</span>';
                } else {
                    return val;
                }

            },
        }, {
            text: "停用碼",
            dataIndex: 'CTDMDCCODE_N',
            style: 'text-align:left',
            align: 'left',
            width: 100,
            renderer: function (val, meta, record) {
                
                if (record.data.DIFFER == 'Y') {
                    return '<span style="color:red">' + val + '</span>';
                } else {
                    return val;
                }

            },
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
                
                if (T1LastRec != null && T1LastRec != undefined) {
                    Ext.ComponentQuery.query('panel[itemId=form]')[0].expand();
                    //ComboData();
                    setFormT1a();
                }
                
                //T11Load();
            }
        }
    });
    function setFormT1a() {

        T1Grid.down('#t1edit').setDisabled(T1Rec === 0);
        // T1Grid.down('#t1delete').setDisabled(T1Rec === 0);
        if (T1LastRec) {
            isNew = false;
            console.log(T1LastRec);
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            //f.findField('MAT_CLASS').setValue({ M_CONTID: T1LastRec.data.MAT_CLASS })
            f.findField('MMCODE').setReadOnly(true);
            f.findField('WH_NO').setReadOnly(true);
            f.findField('PACK_UNIT').setReadOnly(true);
            f.findField('PACK_TIMES').setReadOnly(true);
            //f.findField('PACK_QTY').setReadOnly(true);

            T1Grid.down('#t1edit').setDisabled(false);
            T1Grid.down('#t1delete').setDisabled(false);

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
        }, wh_No,
            MMCode,
            {
                xtype: 'displayfield',
                fieldLabel: '品名',
                name: 'MMNAME_E'
            },
          
        UnitCombo,
        {
            xtype: 'textfield',
            fieldLabel: '申請包裝轉換量',
            readOnly: true,
            name: 'PACK_QTY'

        }, {
            xtype: 'numberfield',
            fieldLabel: '包裝申領量倍數',
            readOnly: true,
            name: 'PACK_TIMES',
            min: 1,
            hideTrigger: true

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
                    Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            //T1Cleanup();
                            T1Query.getForm().reset();
                            var v = action.result.etts[0];
                            T1Query.getForm().findField('P0').setValue(v.WH_NO);
                            T1Query.getForm().findField('P1').setValue(v.MMCODE);
                            T1Query.getForm().findField('P2').setValue(v.MMCODE);
                            T1Load(true);
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            T1Load(false);
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

    //#region differWidow
    // packUnitStore
    var packUnitStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'EXTRA1'],
        data: []
    });
    var findColumnIndex = function (columns, dataIndex) {
        var index;
        for (index = 0; index < columns.length; ++index) {
            if (columns[index].dataIndex == dataIndex) { break; }
        }
        return index == columns.length ? -1 : index;
    }

    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 100,
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    wh_no: T1Query.getForm().getValues()['P0'],
                    ctdmdccode: T2Query.getForm().findField('P3').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0048/GetDifferList',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            },
            timeout: 300000
        }
    });
    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                id: 'update',
                itemId: 'update', text: '儲存',  handler: function () {

                    var tempData = T2Grid.getStore().data.items;
                    var data = [];
                    
                    for (var i = 0; i < tempData.length; i++) {
                        if (tempData[i].dirty) {
                            //if (tempData[i].data.CHK_QTY == '' || tempData[i].data.CHK_QTY == null) {
                            //    var msg = '';
                            //    msg = tempData[i].data.MMCODE + ' 請輸入盤點量'
                            //    Ext.Msg.alert('提示', msg);
                            //    return;
                            //}

                            data.push(tempData[i].data);
                        }
                    }

                    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                    myMask.show();

                    Ext.Ajax.request({
                        url: '/api/AB0048/UpdateDifferList',
                        method: reqVal_p,
                        contentType: "application/json",
                        params: {
                            list: Ext.util.JSON.encode(data)
                        },
                        success: function (response) {
                            
                            var data = Ext.decode(response.responseText);
                            myMask.hide();
                            if (data.success) {

                                var data = JSON.parse(response.responseText);
                                if (data.success == false) {
                                    Ext.Msg.alert('失敗', data.msg);
                                    return;
                                }

                                msglabel('訊息區:資料更新成功');
                                T2Store.load({
                                    params: {
                                        start: 0
                                    }
                                });

                            } else {
                                Ext.Msg.alert('失敗', 'Ajax communication failed');
                            }
                        },

                        failure: function (response, action) {
                            myMask.hide();
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                        }
                    });
                }
            },
        ]
    });
    function T2Load(moveFirst) {
        if (moveFirst) {
            T2Tool.moveFirst();

        } else {
            T2Store.load({
                params: {
                    start: 0
                }
            });
        }
    }

    var T2Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: 200
        },
        items: [{
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'displayfield',
                    fieldLabel: '庫房代碼',
                    name:'wh_no'
                },
                {
                    xtype: 'combo',
                    fieldLabel: '停用碼',
                    name: 'P3',
                    store: CtdmdQueryStore,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        T2Load(true);
                        msglabel('訊息區:');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        msglabel('訊息區:');
                    }
                }
            ]
        }]
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store,
        //selType: 'simple',
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: windowHeight,
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T2Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T2Tool]
            }
        ],
        columns: [
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                style: 'text-align:left',
                align: 'left',
                width: 100,
            }, {
                text: "品名",
                dataIndex: 'MMNAME_E',
                style: 'text-align:left',
                align: 'left',
                width: 150,
              
            }, {
                text: "原申請包裝單位",
                dataIndex: 'PACK_UNIT_ORI',
                style: 'text-align:left',
                align: 'left',
                width: 80,
               
            }, {
                text: "原申請包裝轉換量",
                dataIndex: 'PACK_QTY_ORI',
                style: 'text-align:left',
                align: 'right',
                width: 90,
              
            }, {
                text: "原包裝申領量倍數",
                dataIndex: 'PACK_TIMES_ORI',
                style: 'text-align:left',
                align: 'right',
                width: 120,
               
            }, {
                text: "停用碼",
                dataIndex: 'CTDMDCCODE_N',
                style: 'text-align:left',
                align: 'left',
                width: 100,
              
            }, {
                text: "新申請包裝單位",
                dataIndex: 'PACK_UNIT',
                style: 'text-align:left; color:red',
                align: 'left',
                width: 90,
                editor: {
                    xtype: 'combo',
                    store: packUnitStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    fieldCls: 'required',
                    allowBlank: false,
                    listeners: {

                        select: function (c, r, i, e) {
                            msglabel('');
                            if (r.get('VALUE').trim() != T2LastRec.get('PACK_UNIT')) {
                                //選取下拉項目時，顯示回傳值
                                T2LastRec.set('PACK_UNIT', r.get('VALUE'));
                                T2LastRec.set('PACK_QTY', r.get('EXTRA1'));

                                var total = T2LastRec.data.TOTAL;
                                if (total % T2LastRec.data.PACK_QTY != 0) {
                                    return;
                                }
                                var pack_times = total / T2LastRec.data.PACK_QTY;
                                T2LastRec.set('PACK_TIMES', pack_times);
                            }
                            
                        },
                        focus: function (field, eOpts) {
                            setTimeout(() => getPackUnits(T2LastRec.data.MMCODE), 500);
                        }
                    }
                }
            }, {
                text: "新申請包裝轉換量",
                dataIndex: 'PACK_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 90,
               
            }, {
                text: "新包裝申領量倍數",
                dataIndex: 'PACK_TIMES',
                style: 'text-align:left; color:red',
                align: 'right',
                width: 120,
                editor: {
                    xtype: 'textfield',
                    regexText: '只能輸入數字',
                    regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
                    selectOnFocus: true,
                    listeners: {
                        change: function (field, newVal, oldVal) {
                        },
                    }
                },
            }, {
                header: "",
                flex: 1
            }
        ],
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1,
            })
        ],
        listeners: {
            beforeedit: function (editor, e) {
                
                var editColumnIndex1 = findColumnIndex(T2Grid.columns, 'PACK_UNIT');
                var editColumnIndex2 = findColumnIndex(T2Grid.columns, 'PACK_TIMES');
                
            },
            selectionchange: function (model, records) {

                T2Rec = records.length;
                T2LastRec = records[0];

            }
        },
    });

    function getPackUnits(mmcode) {
        Ext.Ajax.request({
            url: '/api/AB0048/GetPackUnitsCombo',
            method: reqVal_p,
            params: { mmcode: mmcode},
            success: function (response) {
                var data = Ext.decode(response.responseText);

                if (data.success == false) {
                    Ext.Msg.alert('訊息', data.msg);
                    return;
                }
                packUnitStore.removeAll();
                var datas = data.etts;
                
                if (datas.length > 0) {


                    for (var i = 0; i < datas.length; i++) {
                        packUnitStore.add({ VALUE: datas[i].VALUE, TEXT: datas[i].TEXT, EXTRA1: datas[i].EXTRA1 })
                    }
                    
                }
            },
            failure: function (response, options) {

            }
        });
    }

    var differWidow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        id: 'differWidow',
        items: [T2Grid],
        resizable: true,
        draggable: true,
        closable: false,
        title: "需修正項目",
        width: windowWidth,
        height: windowHeight,
        buttons: [
            //{
            //    text: '完成',
            //    handler: function () {
            //        //changeMedUid();
            //    }
            //},
            {
                text: '取消',
                handler: function () {
                    differWidow.hide();
                    T1Load(true);
                }
            }
        ],
        listeners: {
            show: function (self, eOpts) {
                differWidow.center();
                T2Query.getForm().findField('P3').setValue('');
            }
        }
    });
    differWidow.hide();
    //#endregion

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
    //ComboData();
    // getINID(session['UserId']);
    //alert(session['Inid'] );
    T1Query.getForm().findField('P0').focus();

    Ext.on('resize', function () {
        windowHeight = $(window).height();
        windowWidth = $(window).width();
        differWidow.setHeight(windowHeight);
        differWidow.setWidth(windowWidth);
    });
});






