﻿Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var MATQComboGet = '../../../api/BD0019/GetMATQCombo';
    var MATComboGet = '../../../api/BD0019/GetMATCombo';
    var MmcodeComboGet = '../../../api/BD0019/GetMmcodeCombo';
    var Wh_noComboGet = '../../../api/BD0019/GetWh_noCombo';
    var GetTot = '../../../api/BD0019/GetTot';
    var T3GetExcel = '../../../api/BD0019/Excel';

    // var reportUrl = '/Report/B/BD0019.aspx';

    var WH_NO = '';
    var T1LastRec = null, T2LastRec = null; T3LastRec = null;

    var userId = session['UserId'];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName'];
    var nameOfM_CONTID = function () {
        return function (val) {
            if (val == '0')
                return '合約';
            else if (val == '2')
                return '非合約';
            else
                return val;
        }
    }

    var nValInput = '';
    var act_code = '';
    //var userId, userName, userInid, userInidName;

    // 物品類別清單
    var Wh_noQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    // 物品類別清單
    var MATQueryQStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var MATQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    // 院內碼類別清單
    var MmcodeQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });


    function setComboData() {
        Ext.Ajax.request({
            url: MATQComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var comboSt = data.etts;
                    if (comboSt.length > 0) {
                        for (var i = 0; i < comboSt.length; i++) {
                            MATQueryQStore.add({ VALUE: comboSt[i].VALUE, TEXT: comboSt[i].TEXT });
                        }
                        T1Query.getForm().findField('P0').setValue(comboSt[0].VALUE);
                    }
                }
            },
            failure: function (response, options) {

            }
        });

        Ext.Ajax.request({
            url: MATComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var comboSt = data.etts;
                    if (comboSt.length > 0) {
                        for (var i = 0; i < comboSt.length; i++) {
                            MATQueryStore.add({ VALUE: comboSt[i].VALUE, TEXT: comboSt[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    var T2FormMmcode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '院內碼',
        queryUrl: MmcodeComboGet,
        limit: 500,
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                MAT_CLASS: T1Query.getForm().findField('P0').getValue()
            };
        },
        allowBlank: false,
        listeners: {
            change: function (combo, newValue, oldValue, eOpts) {
                var f = T2Form.getForm();
                f.findField('MMNAME_C').setValue('');
                f.findField('MMNAME_E').setValue('');
                f.findField('AGEN_NO').setValue('');
                f.findField('AGEN_NAME').setValue('');
                f.findField('AGEN_FAX').setValue('');
                f.findField('DISC').setValue('');
                f.findField('TOTAL_PRICE').setValue('');
                f.findField('BASE_UNIT').setValue('');
                f.findField('M_PURUN').setValue('');
                f.findField('UNIT_SWAP').setValue('');
                f.findField('M_CONTPRICE').setValue('');
                f.findField('M_CONTID').setValue('');
                f.findField('CASENO').setValue('');
                f.findField('E_SOURCECODE').setValue('');
                f.findField('DISCOUNT_QTY').setValue('');
                f.findField('DISC_CPRICE').setValue('');
                f.findField('PR_PRICE').setValue('');
                if (act_code != '') {
                    Ext.Ajax.request({
                        url: '/api/BD0019/GetSelectMmcodeDetail',
                        method: reqVal_g,
                        params: {
                            MMCODE: newValue,
                            MAT_CLASS: T1Grid.getSelection()[0].get('MAT_CLASS'),
                            WH_NO: T1Query.getForm().findField('WH_NO').getValue()
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                console.log('detail mmcode ajax');
                                var tb_data = data.etts;
                                if (tb_data.length > 0) {
                                    f.findField('MMNAME_C').setValue(tb_data[0].MMNAME_C);
                                    f.findField('MMNAME_E').setValue(tb_data[0].MMNAME_E);
                                    f.findField('AGEN_NO').setValue(tb_data[0].AGEN_NO);
                                    f.findField('AGEN_NAME').setValue(tb_data[0].AGEN_NAMEC);
                                    f.findField('AGEN_FAX').setValue(tb_data[0].AGEN_FAX);
                                    f.findField('DISC').setValue(tb_data[0].M_DISCPERC);

                                    //if (tb_data[0].M_CONTID == '0') {
                                    //    f.findField('M_CONTID0').setValue(true);
                                    //    f.findField('M_CONTID2').setValue(false);
                                    //}
                                    //else {
                                    //    f.findField('M_CONTID0').setValue(false);
                                    //    f.findField('M_CONTID2').setValue(true);
                                    //    GetTotFun(T1Grid.getSelection()[0].get('MAT_CLASS'), tb_data[0].MMCODE, f.findField("TOTAL_PRICE").getValue());
                                    //}
                                    f.findField('BASE_UNIT').setValue(tb_data[0].BASE_UNIT);
                                    f.findField('E_CODATE').setValue(tb_data[0].E_CODATE);
                                    f.findField('E_ITEMARMYNO').setValue(tb_data[0].E_ITEMARMYNO);
                                    f.findField('M_PURUN').setValue(tb_data[0].M_PURUN);
                                    f.findField('UNIT_SWAP').setValue(tb_data[0].UNIT_SWAP);
                                    f.findField('M_CONTPRICE').setValue(tb_data[0].M_CONTPRICE);
                                    f.findField('M_CONTID').setValue(tb_data[0].M_CONTID);
                                    f.findField('CASENO').setValue(tb_data[0].CASENO);
                                    f.findField('E_SOURCECODE').setValue(tb_data[0].E_SOURCECODE);
                                    f.findField('DISCOUNT_QTY').setValue(tb_data[0].DISCOUNT_QTY);
                                    f.findField('DISC_CPRICE').setValue(tb_data[0].DISC_CPRICE);
                                    f.findField('PR_PRICE').setValue(tb_data[0].PR_PRICE);
                                    var pr_qty = f.findField('PR_QTY').getValue();
                                    var setPr_qty = Math.round((parseInt(pr_qty) * parseFloat(tb_data[0].PR_PRICE)) * 100) / 100;
                                    f.findField('TOTAL_PRICE').setValue(setPr_qty);
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
                    if (data.msg > 100000) {
                        Ext.MessageBox.alert('提示', '非合約累計採購金額預計超過(含)十萬元,請修正申購量');
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

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'PR_NO', type: 'string' },
            { name: 'M_STOREID', type: 'string' },
            { name: 'MAT_CLASS', type: 'string' },
            { name: 'PR_DEPT', type: 'string' },
            { name: 'PR_STATUS', type: 'string' },
            { name: 'PR_TIME', type: 'date' },
            { name: 'PR_USER', type: 'string' },
            { name: 'PR_AMT', type: 'string' },
            { name: 'CREATE_TIME', type: 'date' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_TIME', type: 'date' },
            { name: 'UPDATE_IP', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'PR_NO', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BD0019/GetMST',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: '',
                    p4: '0'
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {

        T1Tool.moveFirst();
    }

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'MMCODE', type: 'string' },
            { name: 'PR_NO', type: 'string' },
            { name: 'AGEN_FAX', type: 'string' },
            { name: 'AGEN_NAME', type: 'string' },
            { name: 'AGEN_NO', type: 'string' },
            { name: 'BW_SQTY', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'E_ITEMARMYNO', type: 'string' },
            { name: 'E_CODATE', type: 'string' },
            { name: 'DELI_QTY', type: 'string' },
            { name: 'DISC', type: 'string' },
            { name: 'IS_FAX', type: 'string' },
            { name: 'IS_MAIL', type: 'string' },
            { name: 'M_CONTID', type: 'string' },
            { name: 'M_CONTPRICE', type: 'string' },
            { name: 'M_PURUN', type: 'string' },
            { name: 'PO_NO', type: 'string' },
            { name: 'PR_PRICE', type: 'string' },
            { name: 'PR_QTY', type: 'string' },
            { name: 'REQ_QTY_T', type: 'string' },
            { name: 'UNIT_SWAP', type: 'string' },
            { name: 'CREATE_TIME', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' },
            { name: 'UPDATE_TIME', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPRICE', type: 'string' },
            { name: 'TOTAL_PRICE', type: 'string' },
            { name: 'TOT', type: 'string' },
            { name: 'INV_QTY', type: 'string' },
            { name: 'DISCOUNT_QTY', type: 'string' },
            { name: 'Seq', type: 'string' },
            { name: 'DISC_CPRICE', type: 'string' }
        ]
    });
    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BD0019/GetD',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'//
            }
        },
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
        autoScroll: false, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
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
                            fieldLabel: '庫房號碼',
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
                            matchFieldWidth: false,
                            listConfig: { width: 210 },
                            padding: '0 4 0 4',
                            store: MATQueryQStore,
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
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {
                                var MAT_CLASS = T1Query.getForm().findField('P0').getValue();
                                var tempWH_NO = T1Query.getForm().findField('WH_NO').getValue();
                                var TXTDay_B = T1Query.getForm().findField('P1').getValue();
                                var TXTDay_E = T1Query.getForm().findField('P2').getValue();

                                if (MAT_CLASS != null && MAT_CLASS != '' && tempWH_NO != null && tempWH_NO != '' && TXTDay_B != null && TXTDay_B != '' && TXTDay_E != null && TXTDay_E != '') {
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
    var T1TransOrders = function (pr_no, pr_no_list) {
        Ext.Ajax.request({
            url: '/api/BD0019/GetDetailDataForT1TransOrders',
            method: reqVal_p,
            params: {
                p0: T1Grid.getSelection()[0].get('PR_NO'),
                p1: T2Query.getForm().findField('P0').getValue(),
                wh_no: WH_NO
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (data.etts.length > 0) {
                        Ext.Ajax.request({
                            url: '/api/BD0019/MasterTrans',
                            method: reqVal_p,
                            params: {
                                PR_NO: pr_no,
                                ITEM_STRING: Ext.util.JSON.encode(pr_no_list)
                            },
                            success: function (response) {
                                myMask.hide();
                                var data = Ext.decode(response.responseText);
                                if (data.success) {
                                    Ext.MessageBox.alert('訊息', '申購單轉訂單成功');
                                    msglabel('訊息區:申購單轉訂單成功');
                                    T1Load();
                                }
                                else
                                    Ext.MessageBox.alert('錯誤', data.msg);
                            },
                            failure: function (response) {
                                myMask.hide();
                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            }
                        });
                    }
                    else {
                        myMask.hide();
                        Ext.MessageBox.alert('提醒', '該筆無明細資料，不可轉訂單');
                    }
                }
                else
                    Ext.MessageBox.alert('錯誤', '檢查轉訂單是否有明細資料發生錯誤');
            },
            failure: function (response) {
                Ext.MessageBox.alert('錯誤', '檢查轉訂單是否有明細資料發生錯誤');
            }
        });
    };

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
                    T1Set = '/api/BD0019/MasterCreate';
                    setFormT1('I', '新增');
                }
            },
            {
                text: '修改',
                id: 'btnUpdate',
                name: 'btnUpdate',
                disabled: true,
                handler: function () {
                    setFormVisible(true, false);
                    T1Set = '/api/BD0019/MasterUpdate';
                    setFormT1('U', '修改');
                }
            },
            //{
            //    text: '查詢申購單',
            //    id: 'btnQuery',
            //    name: 'btnQuery',
            //    handler: function () {

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
                            if (pr_no !== '') {
                                pr_no += '<br>';
                            }
                            pr_no += item.get('PR_NO');
                        })

                        Ext.MessageBox.confirm('刪除', '是否確定刪除申購單號?<br>' + pr_no, function (btn, text) {
                            if (btn === 'yes') {
                                myMask.show();
                                Ext.Ajax.request({
                                    url: '/api/BD0019/MasterDelete',
                                    method: reqVal_p,
                                    params: {
                                        PR_NO: pr_no
                                    },
                                    success: function (response) {
                                        myMask.hide();
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:資料刪除成功');

                                            T1Load();
                                        }
                                        else
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                    },
                                    failure: function (response) {
                                        myMask.hide();
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        });
                    }
                }
            },
            {
                text: '轉訂單',
                id: 'btnTrans',
                name: 'btnTrans',
                disabled: true,
                handler: function () {
                    var selection = T1Grid.getSelection();
                    var pr_no = '';
                    if (selection.length) {
                        //selection.map(item => {
                        //    pr_no = item.get('PR_NO');
                        //});
                        pr_no_list = [];
                        $.map(selection, function (item, key) {
                            pr_no += item.get('PR_NO') + '<br>';
                            pr_no_list.push({ PR_NO: item.get('PR_NO') });
                        });
                        myMask.show();
                        var hadShowConfirm = 0;

                        //1121009新竹 檢查出貨單位 不卡關改提示
                        Ext.Ajax.request({
                            url: '/api/BD0019/ChkUnitrate',
                            method: reqVal_p,
                            params: {
                                PR_NO: pr_no,
                                ITEM_STRING: Ext.util.JSON.encode(pr_no_list)
                            },
                            success: function (response) {
                                var data = Ext.decode(response.responseText);
                                if (data.success) {
                                    if (data.msg) {
                                        // 若有訊息則要詢問是否確認
                                        Ext.MessageBox.confirm('轉訂單', data.msg, function (btn, text) {
                                            if (btn === 'yes') {
                                                // 檢查若非合約累計採購金額，自今年一月到上個月(月結檔)，合計超過(含)十五萬元
                                                Ext.Ajax.request({
                                                    url: '/api/BD0019/GetMasterExceedAmtMMCodes',
                                                    method: reqVal_p,
                                                    params: {
                                                        PR_NO: pr_no,
                                                        ITEM_STRING: Ext.util.JSON.encode(pr_no_list)
                                                    },
                                                    success: function (response) {
                                                        var data = Ext.decode(response.responseText);
                                                        myMask.hide();
                                                        if (data.success) {
                                                            if (data.etts) {
                                                                hadShowConfirm = 1;
                                                                Ext.MessageBox.confirm('轉訂單', data.msg, function (btn, text) {
                                                                    if (btn === 'yes') {
                                                                        T1TransOrders(pr_no, pr_no_list);
                                                                    }
                                                                });
                                                            }

                                                            if (!hadShowConfirm) {
                                                                Ext.MessageBox.confirm('轉訂單', '是否確定轉訂單?<br>' + pr_no, function (btn, text) {
                                                                    if (btn === 'yes') {
                                                                        T1TransOrders(pr_no, pr_no_list);
                                                                    }
                                                                });
                                                            }
                                                        } else {
                                                            Ext.MessageBox.alert('錯誤', data.msg);
                                                        }
                                                    },
                                                    failure: function (response) {
                                                        myMask.hide()
                                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                                    }
                                                });
                                            }
                                            else
                                                myMask.hide();
                                        });
                                    }
                                    else {
                                        // 檢查若非合約累計採購金額，自今年一月到上個月(月結檔)，合計超過(含)十五萬元
                                        Ext.Ajax.request({
                                            url: '/api/BD0019/GetMasterExceedAmtMMCodes',
                                            method: reqVal_p,
                                            params: {
                                                PR_NO: pr_no,
                                                ITEM_STRING: Ext.util.JSON.encode(pr_no_list)
                                            },
                                            success: function (response) {
                                                var data = Ext.decode(response.responseText);
                                                myMask.hide();
                                                if (data.success) {
                                                    if (data.etts) {
                                                        hadShowConfirm = 1;
                                                        Ext.MessageBox.confirm('轉訂單', data.msg, function (btn, text) {
                                                            if (btn === 'yes') {
                                                                T1TransOrders(pr_no, pr_no_list);
                                                            }
                                                        });
                                                    }

                                                    if (!hadShowConfirm) {
                                                        Ext.MessageBox.confirm('轉訂單', '是否確定轉訂單?<br>' + pr_no, function (btn, text) {
                                                            if (btn === 'yes') {
                                                                T1TransOrders(pr_no, pr_no_list);
                                                            }
                                                        });
                                                    }
                                                } else {
                                                    Ext.MessageBox.alert('錯誤', data.msg);
                                                }
                                            },
                                            failure: function (response) {
                                                myMask.hide()
                                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                            }
                                        });
                                    }
                                } else {
                                    Ext.MessageBox.alert('錯誤', data.msg);
                                }
                            },
                            failure: function (response) {
                                myMask.hide()
                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            }
                        });
                    }
                }
            },
            {
                text: '申購匯總',
                id: 'btnTotal',
                name: 'btnTotal',
                disabled: true,
                handler: function () {
                    var selection = T1Grid.getSelection();
                    var pr_no = '';
                    if (selection.length) {
                        //selection.map(item => {
                        //    pr_no = item.get('PR_NO');
                        //});
                        pr_no_list = [];
                        $.map(selection, function (item, key) {
                            pr_no += item.get('PR_NO') + '<br>';
                            pr_no_list.push({ PR_NO: item.get('PR_NO') });
                        })

                        Ext.MessageBox.confirm('申購匯總', '是否確定申購匯總?<br>' + pr_no, function (btn, text) {
                            if (btn === 'yes') {
                                myMask.show();
                                Ext.Ajax.request({
                                    url: '/api/BD0019/MasterTotal',
                                    method: reqVal_p,
                                    params: {
                                        PR_NO: pr_no,
                                        ITEM_STRING: Ext.util.JSON.encode(pr_no_list),
                                        MAT_CLASS: T1Query.getForm().findField('P0').getValue()
                                    },
                                    success: function (response) {
                                        myMask.hide();
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:申購匯總成功');

                                            T1Load();
                                        }
                                        else
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                    },
                                    failure: function (response) {
                                        myMask.hide()
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

    Ext.define('selRecords', {
        selectarray: Array(),
        insertarray: function (key) {
            var checkexist = false;
            Ext.Array.each(selRecords.selectarray, function (id) {
                if (id == key) {
                    checkexist = true;
                    return;
                }
            });
            if (!checkexist) {
                selRecords.selectarray.push(key);
            }
        },
        deletearray: function (key) {
            selRecords.selectarray.splice(jQuery.inArray(key, selRecords.selectarray), 1)
        },
        singleton: true
    });
    var checkboxT1Model = Ext.create('Ext.selection.CheckboxModel', {
        listeners: {
            'beforeselect': function (view, rec, index) {
                //if (rec.get('PR_STATUS') != '35') {
                //    // Ext.Msg.alert('訊息', rec.get('PR_NO') + '狀態不為申購單開立');
                //    return false;
                //}
            },
            'select': function (view, rec) {
                if (rec.get('PR_STATUS') == '35')
                    selRecords.insertarray(rec.get('PR_NO'));
                T1ChkBtns();
            },
            'deselect': function (view, rec) {
                selRecords.deletearray(rec.get('PR_NO'));
                T1ChkBtns();
            }
        }
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
                items: [T1Query],
                resizable: true
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        selModel: checkboxT1Model,
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "申購單號",
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
                width: 80,
                format: 'Xmd'
            },
            {
                text: "核可轉申購",
                dataIndex: 'ISFROMDOCM',
                width: 90,
            },
            {
                text: "訂購金額合計",
                dataIndex: 'PR_AMT',
                width: 90
            },
            {
                text: "備註",
                dataIndex: 'MEMO',
                width: 180
            },
            {
                header: "",
                flex: 1
            }
        ],
        viewConfig: {
            listeners: {
                refresh: function (view) {
                    // T1Grid.down('#t1print').setDisabled(true);
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

                    if (T1LastRec.data.PR_STATUS != '34' && T1LastRec.data.PR_STATUS != '36') {
                        Ext.getCmp('btnAdd2').setDisabled(false);
                        //Ext.getCmp('btnUpdate').setDisabled(false);
                        Ext.getCmp('btnDel').setDisabled(false);
                    }
                    else {
                        Ext.getCmp('btnDel').setDisabled(true);
                    }
                    // Ext.getCmp('t1print').setDisabled(false);
                    T2Load();
                }
                T1ChkBtns();
                setFormT1a();
            }
        }
    });

    var T2Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: false, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
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
                    T1Set = '/api/BD0019/DetailCreate';
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
                    T1Set = '/api/BD0019/DetailUpdate';
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
                    var seq = '';
                    if (selection.length) {
                        pr_no = selection[0].get('PR_NO');
                        mmcode = selection[0].get('MMCODE');
                        seq = selection[0].get('Seq');

                        Ext.MessageBox.confirm('刪除', '是否確定刪除申購院內碼?<br>' + mmcode, function (btn, text) {
                            if (btn === 'yes') {
                                myMask.show();
                                Ext.Ajax.request({
                                    url: '/api/BD0019/DetailDelete',
                                    method: reqVal_p,
                                    params: {
                                        PR_NO: pr_no,
                                        MMCODE: mmcode,
                                        SEQ: seq
                                    },
                                    success: function (response) {
                                        myMask.hide();
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:資料刪除成功');
                                            T2Load();
                                            Ext.getCmp('eastform').collapse();
                                        }
                                    },
                                    failure: function (response) {
                                        myMask.hide();
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        });
                    }
                }
            },
            {
                id: 'getexport',
                name: 'getexport',
                text: '匯入',
                disabled: true,
                handler: function () {
                    msglabel('訊息區:');
                    showWin3();
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
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 120
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 120
            },
            {
                text: "單位",
                dataIndex: 'BASE_UNIT',
                width: 60
            },
            {
                text: "包裝",
                dataIndex: 'M_PURUN',
                width: 60
            },
            {
                text: "聯標項次",
                dataIndex: "E_ITEMARMYNO",
                width: 60
            },
            {
                text: "數量",
                dataIndex: 'PR_QTY',
                width: 70,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "申購單價",
                dataIndex: 'PR_PRICE',
                width: 80,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "小計",
                dataIndex: 'TOTAL_PRICE',
                width: 80,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "備註",
                dataIndex: 'MEMO',
                width: 100,
            },
            {
                text: "買斷寄庫",
                dataIndex: 'E_SOURCECODE',
                width: 80,
            },
            {
                text: "合約效期日",
                dataIndex: "E_CODATE",
                width: 80
            },
            {
                text: "是否合約",
                dataIndex: 'M_CONTID',
                width: 90,
                renderer: nameOfM_CONTID()
            },
            {
                text: "合約案號",
                dataIndex: 'CASENO',
                width: 90,
            },
            {
                text: "出貨單位",
                dataIndex: 'UNIT_SWAP',
                width: 100,
                renderer: function (val, meta, record) {

                    return record.data.UNIT_SWAP + ' ' + record.data.BASE_UNIT + '/' + record.data.M_PURUN;
                }
            },
            {
                text: "折讓比",
                dataIndex: 'DISC',
                width: 70,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "成本價",
                dataIndex: 'DISC_CPRICE',
                width: 110,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "決標價",
                dataIndex: 'M_CONTPRICE',
                width: 80,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "二次折讓數量",
                dataIndex: 'DISCOUNT_QTY',
                width: 90,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "廠商代碼",
                dataIndex: 'AGEN_NO',
                width: 90
            },
            {
                text: "廠商名稱",
                dataIndex: 'AGEN_NAME',
                width: 120
            },
            {
                text: "病患姓名",
                dataIndex: 'CHINNAME',
                width: 120
            },
            {
                text: "病歷號",
                dataIndex: 'CHARTNO',
                width: 120
            },
            {
                text: "Seq",
                dataIndex: 'Seq',
                align: 'right',
                width: 70,
                hidden: true
            },
            //{
            //    text: "正常存量",
            //    dataIndex: 'OPER_QTY',
            //    align: 'right',
            //    width: 70,
            //},
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
                    if (T1LastRec.data.PR_STATUS == '35') {
                        if (T2LastRec != null) {
                            Ext.getCmp('btnAdd2').setDisabled(false);
                            Ext.getCmp('btnUpdate2').setDisabled(false);
                            if (T1LastRec.data.ISFROMDOCM == 'N') {
                                Ext.getCmp('btnDel2').setDisabled(false);
                            }
                            else {
                                Ext.getCmp('btnDel2').setDisabled(true);
                            }

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
            f.findField("PR_STATUS").setValue('35');;
            var currentDate = new Date();
            f.findField("PR_TIME").setValue(currentDate);
            //MATQueryStore
            f.findField("MAT_CLASS").setValue(MATQueryStore.getData().items[0].data.VALUE);
            f.findField("CREATE_USER").setValue(userName);
        }

        f.findField("MEMO").setReadOnly(false);

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

            f.findField("PR_STATUS").setValue(T1LastRec.data["PR_STATUS"]);
            f.findField("PR_TIME").setValue(T1LastRec.data["PR_TIME"]);
            f.findField("CREATE_USER").setValue(T1LastRec.data["CREATE_USER"]);

            f.findField("MEMO").setValue(T1LastRec.data["MEMO"]);

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
                fieldLabel: '申購單號',
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
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
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
            },
            {
                xtype: 'textarea',
                fieldLabel: '備註',
                name: 'MEMO',
                readOnly: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '建立人員',
                name: 'CREATE_USER',
                readOnly: true,
                submitValue: false
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

                            T1Load();
                            msglabel(msg);

                            Ext.Msg.alert('訊息', msg);
                            break;
                        case "U":
                            T1Load();
                            msglabel(msg);
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

        checkboxT1Model.deselectAll();
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
            f.findField('AGEN_NO').setValue('');
            f.findField('AGEN_NAME').setValue('');
            f.findField('BASE_UNIT').setValue('');
            f.findField('E_ITEMARMYNO').setValue('');
            f.findField('E_CODATE').setValue('');
            f.findField('M_CONTPRICE').setValue('');
            f.findField('DISC').setValue('');
            f.findField('DISC_CPRICE').setValue('');
            f.findField('M_CONTID').setValue('');
            f.findField('MEMO').setValue('');
            f.findField('CASENO').setValue('');
            f.findField('E_SOURCECODE').setValue('');
            f.findField("MEMO").setReadOnly(false);
            f.findField('CHINNAME').setValue('');
            f.findField("CHINNAME").setReadOnly(false);
            f.findField('CHARTNO').setValue('');
            f.findField("CHARTNO").setReadOnly(false);
            f.findField('DISCOUNT_QTY').setValue('');
            f.findField('PR_PRICE').setValue('');
        }
        else {
            f.findField("PR_QTY").setReadOnly(false);
            f.findField("PR_QTY").clearInvalid();
            f.findField("MEMO").setReadOnly(false);
            f.findField("CHINNAME").setReadOnly(false);
            f.findField("CHARTNO").setReadOnly(false);
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
            console.log('T2LastRec: ' + T2LastRec);
            console.log(T2LastRec);
            act_code = '';
            T2Form.loadRecord(T2LastRec);
            viewport.down('#form').expand();
            var f = T2Form.getForm();
            f.findField("PR_NO").setValue(T2LastRec.data["PR_NO"]);
            f.findField("Seq").setValue(T2LastRec.data["Seq"]);
            //f.findField("AGEN_NAMEC").setValue(T2LastRec.data["AGEN_NAME"]);
            //為了檢查數量合理性，只能顯示原貌
            f.findField('UNIT_SWAP').setValue(T2LastRec.data["UNIT_SWAP"]);
            //f.findField('UNIT_SWAP').setValue(T2LastRec.data["UNIT_SWAP"] + ' ' + T2LastRec.data["BASE_UNIT"] + '/' + T2LastRec.data["M_PURUN"])

            //f.findField("TOTAL_PRICE").setValue(Math.floor(parseInt(T2LastRec.data["REQ_QTY_T"]) * parseFloat(T2LastRec.data["M_CONTPRICE"]) ));

            //if (T2LastRec.data["M_CONTID"] == '0') {
            //    f.findField('M_CONTID0').setValue(true);
            //    f.findField('M_CONTID2').setValue(false);
            //}
            //else {
            //    f.findField('M_CONTID0').setValue(false);
            //    f.findField('M_CONTID2').setValue(true);
            //    GetTotFun(T1Grid.getSelection()[0].get('MAT_CLASS'), f.findField("MMCODE").getValue(), f.findField("TOTAL_PRICE").getValue());
            //}
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
                        fieldLabel: '申購單號',
                        name: 'PR_NO',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true
                    },
                    T2FormMmcode,
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
                        fieldLabel: '單位',
                        name: 'BASE_UNIT',
                        submitValue: true
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '包裝',
                        name: 'M_PURUN',
                        submitValue: true
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '聯標項次',
                        name: 'E_ITEMARMYNO',
                        submitValue: true
                    },
                    {
                        xtype: 'numberfield',
                        fieldLabel: '數量',
                        readOnly: true,
                        name: 'PR_QTY',
                        minValue: 1,
                        fieldCls: 'required',
                        allowBlank: false,
                        listeners: {
                            change: function (_this, newvalue, oldvalue) {
                                if (act_code != '' && newvalue != null) {
                                    var f = T2Form.getForm();
                                    var pr_price = f.findField('PR_PRICE').getValue();
                                    // T2Form.getForm().findField('PR_AMT').setValue(Math.floor(parseInt(newvalue) * parseFloat(m_contprice))); //總金額
                                    T2Form.getForm().findField('TOTAL_PRICE').setValue(Math.round((parseInt(newvalue) * parseFloat(pr_price)) * 100) / 100); //總金額

                                }
                            }
                        }
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '申購單價',
                        name: 'PR_PRICE',
                        submitValue: true
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '小計',
                        name: 'TOTAL_PRICE',
                        submitValue: true
                    }, {
                        xtype: 'textarea',
                        fieldLabel: '備註',
                        name: 'MEMO',
                        readOnly: true
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '買斷寄庫',
                        name: 'E_SOURCECODE',
                        submitValue: true
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '合約到期日',
                        name: 'E_CODATE'
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '是否合約',
                        name: 'M_CONTID',
                        readOnly: true,
                        renderer: nameOfM_CONTID()
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '合約案號',
                        name: 'CASENO',
                        readOnly: true
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '出貨單位',
                        name: 'UNIT_SWAP',
                        readOnly: true,
                    }, {
                        xtype: 'label',
                        text: '數量應為出貨單位的倍數',
                        style: 'color: #ff0000;'
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '折讓比',
                        name: 'DISC',
                        submitValue: true
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '成本價',
                        name: 'DISC_CPRICE',
                        submitValue: true
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '決標價',
                        name: 'M_CONTPRICE',
                        submitValue: true
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '二次折讓數量',
                        name: 'DISCOUNT_QTY',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '廠商代碼',
                        name: 'AGEN_NO',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '廠商名稱',
                        name: 'AGEN_NAME',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '廠商傳真',
                        name: 'AGEN_FAX',
                        enforceMaxLength: true,
                        submitValue: true,
                        readOnly: true,
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '病患姓名',
                        name: 'CHINNAME',
                        enforceMaxLength: true,
                        maxLength: 20,
                        readOnly: true
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '病歷號',
                        name: 'CHARTNO',
                        enforceMaxLength: true,
                        maxLength: 10,
                        readOnly: true
                    }, {
                        xtype: 'hidden',
                        fieldLabel: 'Seq',
                        name: 'Seq',
                        readOnly: true,
                        submitValue: true,
                        // xtype: 'hidden'
                    }
                ]
            }
        ],
        buttons: [
            {
                itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                        var isSub = true;
                        var f = T2Form.getForm();
                        var unit_swap = f.findField('UNIT_SWAP').getValue();
                        var pr_qty = f.findField('PR_QTY').getValue();
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        //1121009新竹 檢查出貨單位 不卡關改提示
                        if (!(unit_swap == null || unit_swap == '' || unit_swap == 0)) {
                            if (pr_qty % unit_swap != 0) {
                                Ext.MessageBox.confirm(confirmSubmit, '數量不能被出貨單位整除，是否確定?', function (btn, text) {
                                    if (btn === 'yes') {
                                        T2Submit();
                                    }
                                });
                            }
                            else
                                T2Submit();
                        } else {
                            Ext.MessageBox.confirm(confirmSubmit, '出貨單位空白，是否確定?', function (btn, text) {
                                if (btn === 'yes') {
                                    T2Submit();
                                }
                            });
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
                    //T1Load();
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
        //f.getFields().each(function (fc) {
        //    if (fc.xtype === "displayfield" || fc.xtype === "textfield") {
        //        fc.setReadOnly(true);
        //    } else if (fc.xtype === "combo" || fc.xtype === "datefield") {
        //        fc.setReadOnly(true);
        //    }
        //});
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
    function T1ChkBtns() {
        var selection = T1Grid.getSelection();
        
        if (selection.length) {
            if (selection.length == 1) {
                if (selection[0].get('PR_STATUS') == '35') {
                    Ext.getCmp('btnAdd2').setDisabled(false);
                    if (selection[0].get('ISFROMDOCM') == 'N')
                        Ext.getCmp('getexport').setDisabled(false);
                    else
                        Ext.getCmp('getexport').setDisabled(true);
                    Ext.getCmp('btnUpdate').setDisabled(false);
                    Ext.getCmp('btnDel').setDisabled(false);
                    Ext.getCmp('btnTrans').setDisabled(false);
                    Ext.getCmp('btnTotal').setDisabled(false);
                }
                else {
                    Ext.getCmp('btnAdd2').setDisabled(true);
                    Ext.getCmp('getexport').setDisabled(true);
                    Ext.getCmp('btnUpdate').setDisabled(true);
                    Ext.getCmp('btnDel').setDisabled(true);
                    Ext.getCmp('btnTotal').setDisabled(true);
                }
            }
            else {
                Ext.getCmp('btnAdd2').setDisabled(true);
                Ext.getCmp('getexport').setDisabled(true);
                Ext.getCmp('btnUpdate').setDisabled(true);
                //   Ext.getCmp('btnTotal').setDisabled(true);
                Ext.getCmp('btnDel').setDisabled(false);
                Ext.getCmp('btnTrans').setDisabled(false);
                for (var i = 0; i < selection.length; i++) {
                    if (selection[i].get('PR_STATUS') != '35') {
                        Ext.getCmp('btnDel').setDisabled(true);
                        Ext.getCmp('btnTrans').setDisabled(true);
                        Ext.getCmp('btnTotal').setDisabled(true);
                    }
                }
            }
        }
        else {
            Ext.getCmp('btnAdd2').setDisabled(true);
            Ext.getCmp('getexport').setDisabled(true);
            Ext.getCmp('btnUpdate').setDisabled(true);
            Ext.getCmp('btnDel').setDisabled(true);
            Ext.getCmp('btnTrans').setDisabled(true);
            Ext.getCmp('btnTotal').setDisabled(true);
        }
    }

    Ext.define('T3Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'PR_NO', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'PR_QTY', type: 'string' },
            { name: 'CHECK_RESULT', type: 'string' }
        ]
    });
    var T3Store = Ext.create('Ext.data.Store', {
        model: 'T3Model',
        pageSize: 1000, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    })

    function T3Load() {
        T3Store.load({
            params: {
                start: 0
            }
        });
    }
    var T3Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 80
        },
        border: false,
        items: [
            {
                xtype: 'button',
                id: 'T3export',
                name: 'T3export',
                text: '下載匯入範本', handler: function () {
                    var p = new Array();
                    p.push({ name: 'p0', value: T1LastRec.data.PR_NO });
                    PostForm(T3GetExcel, p);
                    msglabel('訊息區:匯出完成');
                }
            },
            {
                xtype: 'filefield',
                name: 'T3send',
                id: 'T3send',
                buttonOnly: true,
                buttonText: '匯入',
                width: 40,
                listeners: {
                    change: function (widget, value, eOpts) {
                        //Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                        Ext.getCmp('T3insert').setDisabled(true);
                        T3Store.removeAll();
                        var files = event.target.files;
                        var self = this; // the controller
                        if (!files || files.length == 0) return; // make sure we got something
                        var f = files[0];
                        var ext = this.value.split('.').pop();
                        if (!/^(xls|xlsx)$/.test(ext)) {
                            Ext.MessageBox.alert('提示', '請選擇xlsx或xls檔案！');
                            Ext.getCmp('T3send').fileInputEl.dom.value = '';
                            msglabel("請選擇xlsx或xls檔案！");
                        }
                        else {
                            myMask.show();
                            var formData = new FormData();
                            formData.append("file", f);
                            formData.append("pr_no", T1LastRec.data.PR_NO);
                            var ajaxRequest = $.ajax({
                                type: "POST",
                                url: "/api/BD0019/SendExcel",
                                data: formData,
                                processData: false,
                                //必須false才會自動加上正確的Content-Type
                                contentType: false,
                                success: function (data, textStatus, jqXHR) {
                                    var msg = data.msg.split(',');
                                    if (!data.success) {
                                        T3Store.removeAll();
                                        Ext.MessageBox.alert("提示", msg[0]);
                                        msglabel("訊息區:");
                                        Ext.getCmp('T3insert').setDisabled(true);
                                    }
                                    else {
                                        msglabel("訊息區:檔案讀取成功");
                                        T3Store.loadData(data.etts, false);
                                        if (msg[0] == "True") {
                                            Ext.getCmp('T3insert').setDisabled(false);
                                            Ext.MessageBox.alert("提示", "檢核<span style=\"color: blue; font-weight: bold\">成功</span>，可進行更新動作。" + (msg[1] == "True" ? "<br/>檢核<span style=\"color: red; font-weight: bold\">金額大於15萬</span>。" : ""));
                                        };
                                        if (msg[0] == "False") {
                                            Ext.MessageBox.alert("提示", "檢核<span style=\"color: red; font-weight: bold\">失敗</span>，請依錯誤說明修改Excel檔。" + (msg[1] == "True" ? "<br/>檢核<span style=\"color: red; font-weight: bold\">金額大於15萬</span>。" : ""));
                                        };

                                    }
                                    Ext.getCmp('T3send').fileInputEl.dom.value = '';
                                    myMask.hide();
                                },
                                error: function (jqXHR, textStatus, errorThrown) {
                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                    Ext.getCmp('T3send').fileInputEl.dom.value = '';
                                    Ext.getCmp('T3insert').setDisabled(true);
                                    myMask.hide();
                                }
                            });
                        }
                    }
                }
            }
        ]
    });

    function T3Cleanup() {
        T3Query.getForm().reset();
        msglabel('訊息區:');
    }
    var T3Grid = Ext.create('Ext.grid.Panel', {
        autoScroll: true,
        store: T3Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T3Query]
            }
        ],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "申購單號",
            dataIndex: 'PR_NO',
            width: 130
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100,
            sortable: true
        }, {
            text: "申購數量",
            dataIndex: 'PR_QTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "備註",
            dataIndex: 'MEMO',
            width: 100
        }, {
            text: "病患姓名",
            dataIndex: 'CHINNAME',
            width: 120
        }, {
            text: "病歷號",
            dataIndex: 'CHARTNO',
            width: 120
        }, {
            text: "檢核結果",
            dataIndex: 'CHECK_RESULT',
            width: 300
        },
        {
            header: "",
            flex: 1
        }
        ]
    });

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
    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
    var winActWidth = viewport.width - 10;
    var winActHeight = viewport.height - 10;
    var win3;
    if (!win3) {
        win3 = Ext.widget('window', {
            title: '申購匯入檢核',
            closeAction: 'hide',
            width: winActWidth,
            height: winActHeight,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: [T3Grid],
            buttons: [{
                text: '確定匯入',
                id: 'T3insert',
                name: 'T3insert',
                disabled: true,
                handler: function () {
                    myMask.show();
                    Ext.Ajax.request({
                        url: '/api/BD0019/Insert',
                        method: reqVal_p,
                        params: {
                            data: Ext.encode(Ext.pluck(T3Store.data.items, 'data')),
                            pr_no: T1LastRec.data.PR_NO
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                if (data.msg == "True") {
                                    Ext.MessageBox.alert("提示", "<span style=\"color: red; font-weight: bold\">院內碼</span>不可重複，請修改Excel檔。");
                                    msglabel("訊息區:<span style=\"color: red; font-weight: bold\">院內碼</span>不可重複，請修改Excel檔。");
                                }
                                else {
                                    Ext.MessageBox.alert("提示", "匯入<span style=\"color: blue; font-weight: bold\">完成</span>。");
                                    msglabel("訊息區:匯入<span style=\"color: red; font-weight: bold\">完成</span>");
                                }
                                Ext.getCmp('T3insert').setDisabled(true);
                                T3Store.removeAll();
                                var tmpRec = T1LastRec;
                                T1Load();
                                // T2Load(); // T2Load的動作改為在T1Load一段時間後再嘗試透過select T1Grid的資料來重新載入, 以避免無法保證T1和T2哪個先完成載入
                                setTimeout(function () {
                                    checkboxT1Model.select(tmpRec);
                                }, 1000);
                            }
                            myMask.hide();
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
                                    Ext.Msg.alert('失敗', "匯入失敗");
                                    break;
                            }
                        }
                    });

                    hideWin3();
                }
            }, {
                text: '關閉',
                handler: function () {
                    hideWin3();
                }
            }],
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth > 0) ? winActWidth : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight > 0) ? winActHeight : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth = width;
                    winActHeight = height;
                }
            }
        });
    }
    function showWin3() {
        if (win3.hidden) {
            T3Cleanup();
            T3Store.removeAll();
            win3.setTitle(T1LastRec.data.PR_NO + '申購匯入檢核');
            win3.show();
        }
    }
    function hideWin3() {
        if (!win3.hidden) {
            win3.hide();
            T3Cleanup();
        }
    }

});