Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    //var T3Set = '/api/AA0035/MeDoceUpdate';
    var MATComboGet = '../../../api/BA0002/GetMATCombo';
    var MmcodeComboGet = '../../../api/BA0002/GetMmcodeCombo'; 
    var Wh_noComboGet = '../../../api/BA0002/GetWh_noCombo';
    var GetTot = '../../../api/BA0002/GetTot';
    var reportUrl = '/Report/B/BA0002.aspx';

    var WH_NO = '';
    var T1LastRec = null, T2LastRec = null; T3LastRec = null;

    var userId = session['UserId'];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName'];

    var nValInput = '';
    var act_code = '';
    //var userId, userName, userInid, userInidName;

    //var viewModel = Ext.create('WEBAPP.store.AA.AA0035VM');

    // 物品類別清單
    var Wh_noQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    // 物品類別清單
    var MATQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    // 院內碼類別清單
    var MmcodeQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });


    function setComboData() {
        Ext.Ajax.request({
            url: MATComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        for (var i = 0; i < wh_nos.length; i++) {
                            MATQueryStore.add({ VALUE: wh_nos[i].VALUE, TEXT: wh_nos[i].TEXT });
                        }
                        T1Query.getForm().findField('P0').setValue(wh_nos[0].VALUE);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    var T1FormMmcode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '院內碼',
        queryUrl: MmcodeComboGet,
        limit: 500, //限制一次最多顯示10筆
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                MAT_CLASS: T1Query.getForm().findField('P0').getValue(),
                M_STOREID: T1LastRec.data.M_STOREID
            };
        },
        allowBlank: false,
        listeners: {
            change: function (combo, newValue, oldValue, eOpts) {
                var f = T2Form.getForm();
                f.findField('MMNAME_C').setValue('');
                f.findField('MMNAME_E').setValue('');
                f.findField('AGEN_NO').setValue('');
                f.findField('AGEN_NAMEC').setValue('');
                f.findField('AGEN_FAX').setValue('');
                f.findField('DISC').setValue('');
                f.findField('INV_QTY').setValue('');
                f.findField('TOTAL_PRICE').setValue('');
                f.findField('PR_PRICE').setValue('');
                f.findField('BASE_UNIT').setValue('');
                f.findField('M_PURUN').setValue('');
                f.findField('UNIT_SWAP').setValue('');
                f.findField('M_CONTPRICE').setValue('');
                f.findField('M_CONTID0').setValue(false);
                f.findField('M_CONTID2').setValue(false);
                if (act_code != '') {
                    Ext.Ajax.request({
                        url: '/api/BA0002/GetSelectMmcodeDetail',
                        method: reqVal_g,
                        params: {
                            MMCODE: newValue,
                            MAT_CLASS: T1Grid.getSelection()[0].get('MAT_CLASS'),
                            WH_NO: WH_NO,
                            M_STOREID: T1LastRec.data.M_STOREID,
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                
                                var tb_data = data.etts;
                                if (tb_data.length > 0) {
                                    f.findField('MMNAME_C').setValue(tb_data[0].MMNAME_C);
                                    f.findField('MMNAME_E').setValue(tb_data[0].MMNAME_E);
                                    f.findField('AGEN_NO').setValue(tb_data[0].AGEN_NO);
                                    f.findField('AGEN_NAMEC').setValue(tb_data[0].AGEN_NAMEC);
                                    f.findField('AGEN_FAX').setValue(tb_data[0].AGEN_FAX);
                                    f.findField('DISC').setValue(tb_data[0].M_DISCPERC);
                                    f.findField('INV_QTY').setValue(tb_data[0].INV_QTY);
                                    f.findField('TOTAL_PRICE').setValue(tb_data[0].TOTAL_PRICE);
                                    if (tb_data[0].M_CONTID == '0') {
                                        f.findField('M_CONTID0').setValue(true);
                                        f.findField('M_CONTID2').setValue(false);
                                    }
                                    else {
                                        f.findField('M_CONTID0').setValue(false);
                                        f.findField('M_CONTID2').setValue(true);
                                        GetTotFun(T1Grid.getSelection()[0].get('MAT_CLASS'), tb_data[0].MMCODE, f.findField("TOTAL_PRICE").getValue());
                                    }
                                    f.findField('PR_PRICE').setValue(tb_data[0].UPRICE);
                                    f.findField('BASE_UNIT').setValue(tb_data[0].BASE_UNIT);
                                    f.findField('M_PURUN').setValue(tb_data[0].M_PURUN);
                                    f.findField('UNIT_SWAP').setValue(tb_data[0].UNIT_SWAP);
                                    f.findField('M_CONTPRICE').setValue(tb_data[0].M_CONTPRICE);
                                }
                            }
                        },
                        failure: function (response) {
                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                        }
                    });
                }
            }
        }
    });
    function GetTotFun(v_mat_class, v_mmcode, v_total) {
        Ext.Ajax.request({
            url: GetTot,
            method: reqVal_g,
            params: {
                wh_no: WH_NO,
                mat_class: v_mat_class,
                mmcode: v_mmcode,
                totprice: v_total
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    T2Form.getForm().findField('TOT').setValue(data.msg)
                    if (data.msg > 150000) {
                        Ext.MessageBox.alert('提示', '非合約累計採購金額預計超過(含)十五萬元,請修正申購量');
                    }
                }
            },
            failure: function (response, options) {
            }
        });
    }
    function setComboData3() {
        Ext.Ajax.request({
            url: Wh_noComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_no = data.etts;
                    if (wh_no.length > 0) {
                        for (var i = 0; i < wh_no.length; i++) {
                            Wh_noQueryStore.add({ VALUE: wh_no[i].VALUE, TEXT: wh_no[i].TEXT });
                        }
                        T1Query.getForm().findField('WH_NO').setValue(wh_no[0].VALUE);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData3();



    //var T1Store = viewModel.getStore('MM_PR_M');
    var T1Store = Ext.create('WEBAPP.store.MM_PR_M', { // 定義於/Scripts/app/store/PhVender.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: '1'
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {

        T1Tool.moveFirst();
    }

    var T2Store = Ext.create('WEBAPP.store.MM_PR_D', { // 定義於/Scripts/app/store/PhVender.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    p0: T1Grid.getSelection()[0].get('PR_NO'),
                    p1: T2Query.getForm().findField('P0').getValue(),
                    wh_no: WH_NO
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T2Load() {
        T2Tool.moveFirst();
    }

    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 230;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'container',
            layout: 'hbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP1',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'combo',
                            fieldLabel: '庫房別',
                            name: 'WH_NO',
                            enforceMaxLength: true,
                            labelWidth: 60,
                            width: 170,
                            padding: '0 4 0 4',
                            store: Wh_noQueryStore,
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            queryMode: 'local',
                            anyMatch: true,
                            fieldCls: 'required',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                        },
                        {
                            xtype: 'combo',
                            fieldLabel: '物料類別',
                            name: 'P0',
                            enforceMaxLength: true,
                            labelWidth: 60,
                            width: 170,
                            padding: '0 4 0 4',
                            store: MATQueryStore,
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            queryMode: 'local',
                            anyMatch: true,
                            fieldCls: 'required',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                        },
                        {
                            xtype: 'datefield',
                            fieldLabel: '申購日期',
                            name: 'P1',
                            labelWidth: mLabelWidth,
                            width: 150,
                            padding: '0 4 0 4',
                            fieldCls: 'required',
                            value: new Date()
                        }, {
                            xtype: 'datefield',
                            fieldLabel: '至',
                            name: 'P2',
                            labelWidth: 8,
                            width: 88,
                            padding: '0 4 0 4',
                            labelSeparator: '',
                            fieldCls: 'required',
                            value: new Date()

                        },
                        {
                            xtype: 'radiogroup',
                            name: 'P3',
                            items: [
                                { boxLabel: '庫備', name: 'P3', id: 'P3_1', inputValue: '1', width: 50, checked: true },
                                { boxLabel: '非庫備', name: 'P3', id: 'P3_0', inputValue: '0' }
                            ],
                            width: 120,
                            padding: '0 4 0 4',

                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {
                                var MAT_CLASS = T1Query.getForm().findField('P0').getValue();
                                var tempWH_NO = T1Query.getForm().findField('WH_NO').getValue();
                                var TXTDay_B = T1Query.getForm().findField('P1').getValue();
                                var TXTDay_E = T1Query.getForm().findField('P2').getValue();

                                var f = T1Query.getForm();
                                if (f.isValid())
                                    {
                                //if (MAT_CLASS != null && MAT_CLASS != '' && tempWH_NO != null && tempWH_NO != '' && TXTDay_B != null && TXTDay_B != '' && TXTDay_E != null && TXTDay_E != '') {
                                    WH_NO = tempWH_NO;
                                    msglabel('訊息區:');
                                    T1Load();
                                    //Ext.getCmp('btnUpdate').setDisabled(true);
                                    Ext.getCmp('btnDel').setDisabled(true);
                                    //Ext.getCmp('btnSubmit').setDisabled(true);
                                    //Ext.getCmp('btnAdd2').setDisabled(true);
                                    //Ext.getCmp('btnUpdate2').setDisabled(true);
                                    //Ext.getCmp('btnDel2').setDisabled(true);

                                }
                                else {
                                    Ext.MessageBox.alert('提示', '請輸入必填/必選資料後再進行查詢');
                                }
                                //Ext.getCmp('btnAdd2').setDisabled(true);

                                //Ext.getCmp('eastform').collapse();
                            }
                        },
                        {
                            xtype: 'button',
                            text: '清除',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                            }
                        }]
                }

            ]
        }]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        plain: true,
        buttons: [
            {
                text: '開立申購單',
                id: 'btnAdd',
                name: 'btnAdd',
                handler: function () {

                    setFormVisible(true, false);
                    T1Set = '/api/BA0002/MasterCreate';
                    setFormT1('I', '新增');
                }
            },
            //{
            //    text: '修改',
            //    id: 'btnUpdate',
            //    name: 'btnUpdate',
            //    disabled: true,
            //    handler: function () {
            //        setFormVisible(true, false);
            //        T1Set = '/api/BA0002/MasterUpdate';
            //        setFormT1('U', '修改');
            //    }
            //},
            {
                text: '刪除',
                id: 'btnDel',
                name: 'btnDel',
                disabled: true,
                handler: function () {
                    var selection = T1Grid.getSelection();
                    var pr_no = '';
                    if (selection.length) {
                        //selection.map(item => {
                        //    pr_no = item.get('PR_NO');
                        //});
                        $.map(selection, function (item, key) {
                            pr_no = item.get('PR_NO');
                        })

                        Ext.MessageBox.confirm('刪除', '是否確定刪除對外採購單號?<br>' + pr_no, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/BA0002/MasterDelete',
                                    method: reqVal_p,
                                    params: {
                                        PR_NO: pr_no
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:資料刪除成功');

                                            T1Load();
                                        }
                                        else
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        });
                    }
                }
            },
            {
                id: 't1print', text: '列印', disabled: false, handler: function () {
                    if (T2Store.getCount() > 0)
                        showReport();
                    else
                        Ext.Msg.alert('訊息', '請先建立明細資料');
                }
            }
        ]
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        //plugins: [T1RowEditing],
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T1Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "對外採購單號",
                dataIndex: 'PR_NO',
                width: 180
            },
            {
                text: "物料類別",
                dataIndex: 'MAT_CLASS',
                width: 110,
                renderer: function (value) {
                    var matclassTxt = '';
                    MATQueryStore.each(function (rec) {
                        if (value == rec.get('VALUE')) {
                            matclassTxt = rec.get('TEXT');
                            return false;
                        };
                    });
                    return matclassTxt;
                }
            },
            {
                text: "庫備/非庫備",
                dataIndex: 'M_STOREID',
                width: 100,
                renderer: function (value) {
                    switch (value) {
                        case '1': return '庫備'; break;
                        case '0': return '非庫備'; break;
                        default: return value; break;
                    }
                }
            },
            {
                text: "申購狀態",
                dataIndex: 'PR_STATUS',
                width: 120,
                renderer: function (value) {
                    switch (value) {
                        case '35': return '申購單開立'; break;
                        case '34': return '申購進貨完成'; break;
                        case '36': return '已轉訂單'; break;
                        default: return value; break;
                    }
                }
            },
            {
                xtype: 'datecolumn',
                text: "申購日期",
                dataIndex: 'PR_TIME',
                width: 120,
                format: 'Xmd'
            },
            {
                text: "備註",
                dataIndex: 'MEMO',
                width: 120
            },
            {
                header: "",
                flex: 1
            }
        ],
        viewConfig: {
            listeners: {
                refresh: function (view) {
                    T1Grid.down('#t1print').setDisabled(true);
                }
            }
        },
        listeners: {

            selectionchange: function (model, records) {
                T2Store.removeAll();
                //T3Store.removeAll();

                //msglabel('訊息區:');
                setFormVisible(true, false);
                //if (Ext.getCmp('eastform').collapsed == true) {
                //    Ext.getCmp('eastform').expand();
                //}
                T1Rec = records.length;
                T1LastRec = records[0];

                Ext.getCmp('btnAdd2').setDisabled(true);
                Ext.getCmp('btnUpdate2').setDisabled(true);
                Ext.getCmp('btnDel2').setDisabled(true);
                //Ext.getCmp('btnUpdate').setDisabled(true);
                Ext.getCmp('btnDel').setDisabled(true);

                if (T1LastRec != null) {
                    if (T1LastRec.data.M_STOREID == "0") {
                        T2Load();
                        setFormT1a();
                        return;

                    }

                    if (T1LastRec.data.PR_STATUS != '34' && T1LastRec.data.PR_STATUS != '36') {
                        Ext.getCmp('btnAdd2').setDisabled(false);
                        //Ext.getCmp('btnUpdate').setDisabled(false);
                        Ext.getCmp('btnDel').setDisabled(false);
                    }
                    else {
                        Ext.getCmp('btnDel').setDisabled(true);
                    }
                    Ext.getCmp('t1print').setDisabled(false);
                    T2Load();
                }

                setFormT1a();
            }
        }
    });

    var T2Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'container',
            layout: 'hbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP2',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'textfield',
                            fieldLabel: '院內碼',
                            name: 'P0',
                            enforceMaxLength: true,
                            labelWidth: 60,
                            width: 170,
                            padding: '0 4 0 4'
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {
                                msglabel('訊息區:');
                                T2Store.removeAll();
                                T2Load();
                                Ext.getCmp('btnUpdate2').setDisabled(true);
                                Ext.getCmp('btnDel2').setDisabled(true);
                                Ext.getCmp('eastform').collapse();
                            }
                        },
                        {
                            xtype: 'button',
                            text: '清除',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                            }
                        }
                    ]
                }
            ]
        }]
    });

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        border: false,
        plain: true,
        buttons: [
            //{
            //    text: '查詢',
            //    handler: function () {
            //        Ext.getCmp('eastform').expand();
            //        T1Form.setVisible(false);
            //        T1Query.setVisible(true);
            //    }
            //},
            {
                text: '新增',
                id: 'btnAdd2',
                name: 'btnAdd2',
                disabled: true,
                handler: function () {
                    setFormVisible(false, true);
                    T1Set = '/api/BA0002/DetailCreate';
                    setFormT2('I', '新增');
                }
            },
            {
                text: '修改',
                id: 'btnUpdate2',
                name: 'btnUpdate2',
                disabled: true,
                handler: function () {
                    setFormVisible(false, true);
                    //T1Query.setVisible(false);
                    T1Set = '/api/BA0002/DetailUpdate';
                    setFormT2('U', '修改');
                }
            },
            {
                text: '刪除',
                id: 'btnDel2',
                disabled: true,
                name: 'btnDel2',
                handler: function () {
                    var selection = T2Grid.getSelection();
                    var pr_no = '';
                    var mmcode = '';
                    if (selection.length) {
                        pr_no = selection[0].get('PR_NO');
                        mmcode = selection[0].get('MMCODE');

                        Ext.MessageBox.confirm('刪除', '是否確定刪除申購院內碼?<br>' + mmcode, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/BA0002/DetailDelete',
                                    method: reqVal_p,
                                    params: {
                                        PR_NO: pr_no,
                                        MMCODE: mmcode
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:資料刪除成功');
                                            T2Load();
                                            Ext.getCmp('eastform').collapse();
                                        }
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        });
                    }
                }
            }
        ]
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: '核撥明細',
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
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
                xtype: 'rownumberer'
            },
            //{
            //    text: "項次",
            //    dataIndex: 'SEQ',
            //    width: 60,
            //    align: 'right',
            //},
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 80
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 150
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 200
            },
            {
                text: "申購量(包裝量)",
                dataIndex: 'REQ_QTY_T',
                width: 100,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "最小計量單位",
                dataIndex: 'BASE_UNIT',
                width: 80
            },
            {
                text: "包裝單位",
                dataIndex: 'M_PURUN',
                width: 70
            },
            {
                text: "轉換率",
                dataIndex: 'UNIT_SWAP',
                width: 60,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "總金額",
                dataIndex: 'TOTAL_PRICE',
                width: 80,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "合約價",
                dataIndex: 'M_CONTPRICE',
                width: 70,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "申購數量",
                dataIndex: 'PR_QTY',
                width: 70,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "原始彙總申購數量",
                dataIndex: 'SRC_PR_QTY',
                width: 100,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "庫房自留數量",
                dataIndex: 'DIFF_PR_QTY',
                width: 100,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "最小單價",
                dataIndex: 'PR_PRICE',
                width: 70,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "折讓比",
                dataIndex: 'DISC',
                align: 'right',
                width: 60,
            },
            {
                text: "庫存量",
                dataIndex: 'INV_QTY',
                align: 'right',
                width: 70,
            },
            {
                text: "廠商名稱",
                dataIndex: 'AGEN_NAME',
                width: 150
            },
            {
                text: "廠商碼",
                dataIndex: 'AGEN_NO',
                width: 70,
            },
            {
                text: "廠商傳真號碼",
                dataIndex: 'AGEN_FAX',
                width: 100,
            },
            {
                text: "合約碼",
                dataIndex: 'M_CONTID',
                width: 80,
                renderer: function (val, meta, record) {
                    if (val == '0')
                        return '合約';
                    else if (val == '2')
                        return '非合約';
                    else
                        return val;
                }
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            selectionchange: function (model, records) {
                setFormVisible(false, true);
                T2Rec = records.length;
                T2LastRec = records[0];
                if (T1LastRec != null) {
                    if (T1LastRec.data.PR_STATUS != '34' && T1LastRec.data.PR_STATUS != '36') {
                        if (T2LastRec != null) {

                            if (T1LastRec.data.IS_CR == 'Y') {
                                Ext.getCmp('btnAdd2').disable();
                                Ext.getCmp('btnUpdate2').disable();
                                Ext.getCmp('btnDel2').disable();
                                setFormT2a();
                                return;
                            }

                            if (T1LastRec.data.M_STOREID == "0") {
                                Ext.getCmp('btnAdd2').disable();
                                Ext.getCmp('btnDel2').enable();
                            } else {
                                Ext.getCmp('btnDel2').setDisabled(false);
                            }
                            Ext.getCmp('btnUpdate2').setDisabled(false);
                        }
                    }
                }
                setFormT2a();
            }
            //click: {
            //    element: 'el',
            //    fn: function () {
            //        if (T1LastRec != null) {
            //            if (T1LastRec.data.PR_STATUS != '34' && T1LastRec.data.PR_STATUS != '36') {
            //                if (T2LastRec != null) {
            //                    Ext.getCmp('btnUpdate2').setDisabled(false);
            //                    Ext.getCmp('btnDel2').setDisabled(false);
            //                }
            //            }
            //        }
            //        setFormT2a();

            //    }
            //}
        }
    });




    var setFormVisible = function (t1Form, t2Form) {
        T1Form.setVisible(t1Form);
        T2Form.setVisible(t2Form);
    }

    // 按'新增'或'修改'時的動作
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#t2Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.MM_PR_M'); // /Scripts/app/model/MiUnitexch.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            f.findField("PR_NO").setValue('(系統自編)');
            f.findField("MAT_CLASS").setReadOnly(false);
            f.findField("MAT_CLASS").clearInvalid();
            f.findField("M_STOREID").setReadOnly(false);
            f.findField("PR_STATUS").setValue('35');;
            var currentDate = new Date();
            f.findField("PR_TIME").setValue(currentDate);

        }

        if (x === 'U') {
            f.findField("MAT_CLASS").setReadOnly(false);
            f.findField("MAT_CLASS").clearInvalid();
            f.findField("M_STOREID").setReadOnly(false);
        }

        f.findField('x').setValue(x);
        //f.findField('FRWH').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        //u.focus();
    }

    // 點選T1Grid一筆資料的動作
    function setFormT1a() {

        if (T1LastRec != null) {
            viewport.down('#form').expand();
            var f = T1Form.getForm();
            f.findField("PR_NO").setValue(T1LastRec.data["PR_NO"]);
            f.findField("MAT_CLASS").setValue(T1LastRec.data["MAT_CLASS"]);
            if (T1LastRec.data["M_STOREID"] == 1) {
                f.findField("M_STOREID1").setValue(true);
                f.findField("M_STOREID0").setValue(false);
            }
            else {
                f.findField("M_STOREID1").setValue(false);
                f.findField("M_STOREID0").setValue(true);

            }
            f.findField("PR_STATUS").setValue(T1LastRec.data["PR_STATUS"]);
            f.findField("PR_TIME").setValue(T1LastRec.data["PR_TIME"]);


            T1Form.down('#cancel').setVisible(false);
            T1Form.down('#submit').setVisible(false);
        }

    }

    // 顯示明細/新增/修改輸入欄
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
        items: [
            {
                fieldLabel: 'Update',
                name: 'x',
                xtype: 'hidden'
            },
            {
                xtype: 'displayfield',
                fieldLabel: '對外採購表單編號',
                name: 'PR_NO',
                enforceMaxLength: true,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'combo',
                fieldLabel: '物料類別',
                name: 'MAT_CLASS',
                enforceMaxLength: true,
                readOnly: true,
                store: MATQueryStore,
                displayField: 'TEXT',
                valueField: 'VALUE',
                allowBlank: false,
                queryMode: 'local',
                anyMatch: true,
                fieldCls: 'required',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',


            },
            {
                xtype: 'radiogroup',
                name: 'M_STOREID',
                fieldLabel: '庫備/非庫備',
                items: [
                    { boxLabel: '庫備', name: 'M_STOREID', id: 'M_STOREID1', readOnly: true, inputValue: '1', width: 70, checked: true },
                    { boxLabel: '非庫備', name: 'M_STOREID', id: 'M_STOREID0', readOnly: true, inputValue: '0', hidden:true }
                ]
            },
            {
                xtype: 'displayfield',
                fieldLabel: '申購狀態',
                name: 'PR_STATUS',
                enforceMaxLength: true,
                readOnly: true,
                renderer: function (value) {
                    switch (value) {
                        case '35': return '申購單開立'; break;
                        case '34': return '申購進貨完成'; break;
                        case '36': return '已轉訂單'; break;
                        default: return value; break;
                    }

                }
            },
            {
                xtype: 'displayfield',
                fieldLabel: '申購日期',
                name: 'PR_TIME',
                enforceMaxLength: true,
                readOnly: true,
                renderer: function (value) {
                    return Ext.util.Format.date(value, 'Xmd');
                }
            }
        ],
        buttons: [
            {
                itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    if (this.up('form').getForm().isValid()) {
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T1Submit();
                            }
                        });
                    }
                    else
                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
                }
            },
            {
                itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
            }
        ]
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
                    var msg = action.result.msg;

                    switch (f2.findField("x").getValue()) {
                        case "I":
                            var TQ = T1Query.getForm();

                            TQ.findField('P0').setValue(f2.findField('MAT_CLASS').getValue());
                            TQ.findField('P1').setValue(new Date());
                            TQ.findField('P2').setValue(new Date());

                            if (f2.findField('M_STOREID1').checked) {
                                TQ.findField('P3_1').setValue(true);
                                TQ.findField('P3_0').setValue(false);
                            }
                            else {
                                TQ.findField("P3_1").setValue(false);
                                TQ.findField("P3_0").setValue(true);
                            }

                            T1Load();
                            msglabel(msg);

                            Ext.Msg.alert('訊息', msg);
                            break;
                        case "U":
                            msglabel('訊息區:資料更新成功');
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
        viewport.down('#t2Grid').unmask();
        Ext.getCmp('eastform').collapse();
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if (fc.xtype === "displayfield" || fc.xtype === "textfield") {
                fc.setReadOnly(true);
            } else if (fc.xtype === "combo" || fc.xtype === "datefield") {
                fc.setReadOnly(true);
            }
        });
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        //setFormT1a();
    }

    // 按'新增'或'修改'時的動作
    function setFormT2(x, t) {
        act_code = x;
        viewport.down('#t1Grid').mask();
        viewport.down('#t2Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        
        var f = T2Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.MM_PR_D'); // /Scripts/app/model/MiUnitexch.js
            T2Form.loadRecord(r); // 建立空白model,在新增時載入T2Form以清空欄位內容
            f.findField("MMCODE").setReadOnly(false);
            f.findField("MMCODE").clearInvalid();
            f.findField("PR_QTY").setReadOnly(false);
            f.findField("PR_QTY").clearInvalid(); 
            f.findField('PR_NO').setValue(T1Grid.getSelection()[0].get('PR_NO'));
            f.findField('MMNAME_C').setValue('');
            f.findField('MMNAME_E').setValue('');
            f.findField('AGEN_NAMEC').setValue('');
            f.findField('INV_QTY').setValue('');

            f.findField('PR_QTY').setMinValue(1);

            if (T1Grid.getSelection()[0].get('M_STOREID') == 1) {
                f.findField("T2M_STOREID1").setValue(true);
                f.findField("T2M_STOREID0").setValue(false);
            }
            else {
                f.findField("T2M_STOREID1").setValue(false);
                f.findField("T2M_STOREID0").setValue(true);
            }
        }
        else {
            f.findField("PR_QTY").setReadOnly(false);
            f.findField("PR_QTY").clearInvalid();

            f.findField('PR_QTY').setMinValue(f.findField('SRC_PR_QTY').getValue());
        }
        f.findField('x').setValue(x);

        T2Form.down('#cancel').setVisible(true);
        T2Form.down('#submit').setVisible(true);
        //u.focus();
    }
    //function meDocdSeqPromise(docno) {
    //    var deferred = new Ext.Deferred();
    //    

    //    Ext.Ajax.request({
    //        url: GetMeDocdMaxSeq,
    //        method: reqVal_p,
    //        params: {
    //            DOCNO: docno
    //        },
    //        success: function (response) {
    //            deferred.resolve(response.responseText);
    //        },

    //        failure: function (response) {
    //            deferred.reject(response.status);
    //        }
    //    });

    //    return deferred.promise; //will return the underlying promise of deferred

    //}

    // 點選T2Grid一筆資料的動作
    function setFormT2a() {
        if (T2LastRec != null) {
            act_code = '';
            T2Form.loadRecord(T2LastRec);
            viewport.down('#form').expand();
            var f = T2Form.getForm();
            f.findField("AGEN_NAMEC").setValue(T2LastRec.data["AGEN_NAME"]);
            //f.findField("TOTAL_PRICE").setValue(Math.floor(parseInt(T2LastRec.data["REQ_QTY_T"]) * parseFloat(T2LastRec.data["M_CONTPRICE"]) ));
            if (T1Grid.getSelection()[0].get('M_STOREID') == 1) {
                f.findField("T2M_STOREID1").setValue(true);
                f.findField("T2M_STOREID0").setValue(false);
            }
            else {
                f.findField("T2M_STOREID1").setValue(false);
                f.findField("T2M_STOREID0").setValue(true);
            }

            if (T2LastRec.data["M_CONTID"] == '0') {
                f.findField('M_CONTID0').setValue(true);
                f.findField('M_CONTID2').setValue(false);
            }
            else {
                f.findField('M_CONTID0').setValue(false);
                f.findField('M_CONTID2').setValue(true);
                GetTotFun(T1Grid.getSelection()[0].get('MAT_CLASS'), f.findField("MMCODE").getValue(), f.findField("TOTAL_PRICE").getValue());
            }
            T2Form.down('#cancel').setVisible(false);
            T2Form.down('#submit').setVisible(false);
        }

    }

    // 顯示明細/新增/修改輸入欄
    var T2Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        hidden: true,
        cls: 'T2b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 80
        },
        items: [
            {
                xtype: 'container',
                items: [
                    {
                        fieldLabel: 'Update',
                        name: 'x',
                        xtype: 'hidden'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '對外採購表單編號',
                        name: 'PR_NO',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true
                    },
                    T1FormMmcode,
                    {
                        xtype: 'radiogroup',
                        fieldLabel: '庫備/非庫備',
                        name: 'M_STOREID',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        items: [
                            { boxLabel: '庫備', name: 'M_STOREID', id: 'T2M_STOREID1', readOnly: true, inputValue: '1', width: 70, checked: true },
                            { boxLabel: '非庫備', name: 'M_STOREID', id: 'T2M_STOREID0', readOnly: true, inputValue: '0'}
                        ]
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '中文品名',
                        name: 'MMNAME_C',
                        enforceMaxLength: true,
                        maxLength: 13,
                        submitValue: true,

                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '英文品名',
                        name: 'MMNAME_E',
                        submitValue: true
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '廠商碼',
                        name: 'AGEN_NO',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '廠商中文名稱',
                        name: 'AGEN_NAMEC',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '廠商傳真',
                        name: 'AGEN_FAX',
                        enforceMaxLength: true,
                        submitValue: true,
                        readOnly: true,
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '折讓比',
                        name: 'DISC',
                        submitValue: true
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '庫存量',
                        name: 'INV_QTY',
                        submitValue: true
                    },
                    {
                        xtype: 'radiogroup',
                        fieldLabel: '是否合約',
                        name: 'M_CONTID',
                        enforceMaxLength: true,
                        readOnly: true,
                        items: [
                            { boxLabel: '是', name: 'M_CONTID', id: 'M_CONTID0', readOnly: true, inputValue: '0', width: 70, checked: true },
                            { boxLabel: '否', name: 'M_CONTID', id: 'M_CONTID2', readOnly: true, inputValue: '2' }
                        ]
                    },
                    {
                        xtype: 'numberfield',
                        fieldLabel: '申購量(最小計量)',
                        readOnly: true,
                        name: 'PR_QTY',
                        minValue: 1,
                        fieldCls: 'required',
                        allowBlank: false,
                        listeners: {
                            change: function (_this, newvalue, oldvalue) {
                                if (act_code != '' && newvalue != null) {
                                    var f = T2Form.getForm();
                                    var unit_swap = f.findField('UNIT_SWAP').getValue();
                                    var m_contprice = f.findField('M_CONTPRICE').getValue();
                                    var req_qty_y = parseInt(newvalue) / parseInt(unit_swap);
                                    T2Form.getForm().findField('REQ_QTY_T').setValue(req_qty_y);  //申購量
                                    if (req_qty_y != "NaN")
                                        T2Form.getForm().findField('TOTAL_PRICE').setValue(Math.floor(parseInt(req_qty_y) * parseFloat(m_contprice))); //總金額
                                    else
                                        T2Form.getForm().findField('TOTAL_PRICE').setValue(0);
                                    T2Form.getForm().findField('TOT').setValue('');
                                    var tot_price = T2Form.getForm().findField('TOTAL_PRICE').getValue();
                                    if (T2Form.getForm().findField('M_CONTID').getValue() == "2") {
                                        GetTotFun(T1Grid.getSelection()[0].get('MAT_CLASS'), f.findField('MMCODE').getValue(), f.findField("TOTAL_PRICE").getValue());
                                    }
                                }
                            }
                        }
                    },
                    {
                        xtype: 'displayfield',
                        value: '<span style=color:red>[申購量]必須為[轉換率]倍數</span>',

                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '最小單價',
                        name: 'PR_PRICE',
                        submitValue: true
                        //renderren
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '最小計量單位',
                        name: 'BASE_UNIT',
                        submitValue: true
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '包裝單位',
                        name: 'M_PURUN',
                        submitValue: true
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '轉換率',
                        name: 'UNIT_SWAP',
                        submitValue: true
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '申購量(包裝量)',
                        name: 'REQ_QTY_T',
                        submitValue: true
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '合約價',
                        name: 'M_CONTPRICE',
                        submitValue: true
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '總金額',
                        name: 'TOTAL_PRICE',
                        submitValue: true
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '非合約品項累計進貨金額',
                        name: 'TOT',
                        submitValue: true
                    }
                    ,
                    {
                        xtype: 'hidden',
                        fieldLabel: '原始彙總數量',
                        name: 'SRC_PR_QTY',
                        submitValue: true
                    }
                ]
            },
        ],
        buttons: [
            {
                itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        var f = T2Form.getForm();
                        var unit_swap = f.findField('UNIT_SWAP').getValue();
                        var pr_qty = f.findField('PR_QTY').getValue();
                        if (!(unit_swap == null || unit_swap == '' || unit_swap == 0)) {
                            if (pr_qty % unit_swap == 0) {
                                        T2Submit();
                            }
                            else {
                                Ext.Msg.alert('提醒', '數量不能被轉換率整除,請重新輸入');
                            }
                        } else {
                            Ext.Msg.alert('提醒', '請確認轉換率是否正確');
                        }
                    }
                    else
                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
                }
            },
            {
                itemId: 'cancel', text: '離開', hidden: true, handler: T2Cleanup
            }
        ]
    });

    //function chkMMCODE(parMmcode) {
    //    var wh_no = T2Form.getForm().findField('WH_NO').getValue();
    //    Ext.Ajax.request({
    //        url: GetMmdataByMmcode,
    //        method: reqVal_p,
    //        params: { mmcode: parMmcode, wh_no: wh_no },
    //        success: function (response) {
    //            var data = Ext.decode(response.responseText);

    //            if (data.success == false) {
    //                Ext.Msg.alert('訊息', data.msg);
    //                T2Form.getForm().findField('MMCODE').setValue('');
    //                T2Form.getForm().findField('MMNAME_C').setValue('');
    //                T2Form.getForm().findField('MMNAME_E').setValue('');
    //                return;
    //            }

    //            if (data.success) {
    //                var tb_data = data.etts;
    //                if (tb_data.length > 0) {
    //                    T2Form.getForm().findField('MMNAME_C').setValue(tb_data[0].MMNAME_C);
    //                    T2Form.getForm().findField('MMNAME_E').setValue(tb_data[0].MMNAME_E);
    //                }
    //                else {
    //                    Ext.Msg.alert('訊息', '院內碼不存在,請重新輸入!');
    //                    T2Form.getForm().findField('MMCODE').setValue('');
    //                    T2Form.getForm().findField('MMNAME_C').setValue('');
    //                    T2Form.getForm().findField('MMNAME_E').setValue('');
    //                }
    //            }
    //        },
    //        failure: function (response, options) {

    //        }
    //    });
    //}
    function T2Submit() {
        var f = T2Form.getForm();

        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    var f2 = T2Form.getForm();

                    switch (f2.findField("x").getValue()) {
                        case "I":
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            msglabel('訊息區:資料修改成功');
                            Ext.getCmp('eastform').collapse();
                            break;
                    }
                    if (f2.findField("x").getValue() == "I") {
                        setFormT2('I', '新增');
                    }
                    else {
                        T2Cleanup();
                    }
                    myMask.hide();
                    T2Load();
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

    function T2Cleanup() {
        viewport.down('#t1Grid').unmask();
        viewport.down('#t2Grid').unmask();
        //viewport.down('#t3Grid').unmask();
        Ext.getCmp('eastform').collapse();
        var f = T2Form.getForm();
        f.reset();

        f.getFields().each(function (fc) {
            fc.setReadOnly(true);
        });
        T2Form.down('#cancel').hide();
        T2Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        Ext.getCmp('btnUpdate2').setDisabled(true);
        Ext.getCmp('btnDel2').setDisabled(true);
        T2Grid.getSelectionModel().deselectAll();
    }

    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?PR_NO=' + T1Grid.getSelection()[0].get('PR_NO') + '&INID=' + userInid + '&InidName=' + userInidName + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);

        }
        win.show();
    }

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [
            {
                itemId: 't1top',
                region: 'center',
                layout: 'border',
                collapsible: false,
                title: '',
                border: false,
                items: [
                    {
                        itemId: 't1Grid',
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        border: false,
                        height: '40%',
                        split: true,
                        items: [T1Grid]
                    },
                    {
                        itemId: 't2Grid',
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '60%',
                        split: true,
                        items: [T2Grid]
                    }
                ]
            },
            {
                itemId: 'form',
                id: 'eastform',
                region: 'east',
                collapsible: true,
                floatable: true,
                width: 300,
                title: '瀏覽',
                border: false,
                collapsed: true,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [T1Form, T2Form]
            }
        ]
    });

});