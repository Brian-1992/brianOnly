﻿Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var T1Name = '盤點輸入作業';
    var T1F1 = '';
    var T1F2 = '';
    var urlCE0041Excel = '../../../api/CE0041/Excel';
    var T3GetExcel = '../../../api/CE0041/Excel3';
    var mLabelWidth = 70;
    var mWidth = 230;
    var T1LastRec = null;
    var T1Rec = 0;
    var p0_default = "";

    var st_setym = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/CE0041/GetSetymCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                if (DataCount > 0) {
                    p0_default = store.getAt(0).get('VALUE');
                    setYm();
                }
            }
        },
        autoLoad: true
    });
    var is顯示調整消耗欄位 = false;
    var is花蓮且衛星庫房 = false;


    var st_whno = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/CE0041/GetWhNoCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    //是否常用品項
    var st_isIV = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/CE0041/GetIsIVCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {

                if (store.getCount() > 0) {
                    T1Query.getForm().findField('ISIV').setValue(store.getAt(0).get('VALUE'));
                }
            }
        },
        autoLoad: true
    });

    function showReport(url, chk_no, chk_nos) {
        if (!win) {

            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + url + '?CHK_NO=' + chk_no
                + '&PRINT_ORDER=MMCODE' // + T1Query.getForm().findField('P1').getValue() // 庫房代碼 //T24Form.getForm().findField('print_order').getValue()
                + '&CHK_NOS=' + chk_nos
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                        myMask.hide();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 300, viewport.height - 20);
        }
        win.show();
    }

    //function getSetYm() {
    //    Ext.Ajax.request({
    //        url: '/api/CE0041/GetSetYm',
    //        method: reqVal_g,
    //        success: function (response) {
    //            var data = Ext.decode(response.responseText);
    //            if (data.success) {
    //                var list = data.etts;
    //                if (list.length > 0) {
    //                    var item = list[0];
    //                    T1Query.getForm().findField('P0').setValue(item.SET_YM);

    //                }
    //            }
    //        },
    //        failure: function (response, options) {

    //        }
    //    });
    //}

    function setYm() {
        T1Query.getForm().findField('P0').setValue(p0_default);
    }

    function GetChkNoSt() {
        Ext.Ajax.request({
            url: '/api/CE0041/GetChkNoSt',
            params: {
                p0: T1Query.getForm().findField('P0').getValue(), // 盤點年月 
                p1: T1Query.getForm().findField('P1').getValue() // 庫房代碼
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var list = data.etts;
                    if (list.length > 0) {
                        debugger
                        var item = list[0];
                        T1F1 = item.CHK_NO;
                        T1Query.getForm().findField('CHK_NO').setValue(item.CHK_NO);
                        T1Query.getForm().findField('CHK_STATUS').setValue(item.CHK_STATUS);
                        T1Query.getForm().findField('CHK_STATUS_T').setValue(item.CHK_STATUS_T);
                        T1Query.getForm().findField('CHK_WH_KIND').setValue(item.CHK_WH_KIND);
                        T1Query.getForm().findField('HospCode').setValue(item.HospCode);
                        T1Query.getForm().findField('CAN_CHK_ENDTIME').setValue(item.CAN_CHK_ENDTIME);
                        T1Query.getForm().findField('SET_CTIME').setValue(item.SET_CTIME);
                        T1Load();
                    }
                    else {
                        T1Query.getForm().findField('CHK_NO').setValue('');
                        T1Query.getForm().findField('CHK_STATUS').setValue('');
                        T1Query.getForm().findField('CHK_STATUS_T').setValue('');
                        //T1Query.getForm().findField('LAST_UPDATE').setValue('');
                        Ext.MessageBox.alert('訊息', '查無盤點單開立資料，請重新查詢');
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    var drawCellColor = function () {
        return function (val, meta, record) {
            return isBalance(val, meta, record)
                ? '<font color=red>' + val + '</font>'
                : val;
        };
    };

    // CHK_TIME(盤點時間) 不為 空(表示有盤點) 且 有差異(差異量不為0)，再整筆顯示紅色
    // onchange 裡面材能用
    var isBalance = function (val, meta, record) {

        if (record.data['CHK_TIME_STRING'] != '' && record.data['INVENTORY'] != '0' && record.data['INVENTORY'] != '' && record.data['INVENTORY'] != null)
            //return (!$.inArray(record.data['INVENTORY'], ['0', '']) && record.data['CHK_TIME_STRING'] != '');
            return true;
    };

    var getT1QueryHieght = function () {
        return 125;
    }

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        height: getT1QueryHieght(),
        border: false,
        resizable: true,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right'
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP1',
                    border: false,
                    layout: 'hbox',
                    autoHeight:true,
                    items: [
                        {
                            xtype: 'combo',
                            fieldLabel: '盤點年月',
                            name: 'P0',
                            id: 'P0',
                            store: st_setym,
                            labelWidth: mLabelWidth,
                            fieldCls: 'required',
                            allowBlank: false, // 欄位為必填
                            width: 180,
                            displayField: 'TEXT',
                            valueField: 'VALUE'
                        },
                        {
                            xtype: 'combo',
                            fieldLabel: '庫房代碼',
                            name: 'P1',
                            id: 'P1',
                            store: st_whno,
                            fieldCls: 'required',
                            allowBlank: false, // 欄位為必填
                            width: 320,
                            queryMode: 'local',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                            displayField: 'COMBITEM',
                            valueField: 'VALUE',
                            listeners: {
                                change: function () {
                                    Ext.getCmp('T1btn1').setDisabled(false); // 啟用【查詢按鈕】

                                    // 顯示【調整消耗】欄位, **「調整消耗」僅藥局可看到 (藥局：chk_wh_kind = '0' and chk_wh_grade = '2')
                                    is顯示調整消耗欄位 = false;
                                    var ALTERED_USE_QTY__Column = T1Grid.down('gridcolumn[dataIndex=ALTERED_USE_QTY]');
                                    var THEORY_QTY__Column = T1Grid.down('gridcolumn[dataIndex=THEORY_QTY]');
                                    var sel = this.getSelection(); // 庫房代碼值

                                    if (sel) {
                                        if (sel.data.EXTRA1 == "y")
                                            is顯示調整消耗欄位 = true;
                                    }

                                    ALTERED_USE_QTY__Column.setHidden(!is顯示調整消耗欄位);
                                    THEORY_QTY__Column.setHidden(!is顯示調整消耗欄位);

                                    // 「單位請領基準量」、「下月預估申請量」僅花蓮衛星庫房可看到
                                    // 花蓮：(select data_value from PARAM_D where grp_cpde = 'HOSP_INFO' and data_name = 'HospCode') = 805
                                    // 衛星庫房：chk_wh_kind = '1' and chk_wh_grade = '2'
                                    is花蓮且衛星庫房 = false;
                                    var 單位請領基準量__Column = T1Grid.down('gridcolumn[dataIndex=G34_MAX_APPQTY]'); // 單位請領基準量
                                    var 下月預估申請量__Column = T1Grid.down('gridcolumn[dataIndex=EST_APPQTY]'); // 下月預估申請量
                                    if (sel) {
                                        if (sel.data.EXTRA2 == "y")
                                            is花蓮且衛星庫房 = true;
                                    }
                                    單位請領基準量__Column.setHidden(!is花蓮且衛星庫房);
                                    下月預估申請量__Column.setHidden(!is花蓮且衛星庫房);

                                    // 一鍵符合  A.所選擇庫房代碼為藥局才可看到此按鈕
                                    var is庫房代碼為藥局 = false;
                                    sel = this.getSelection();
                                    if (sel) {
                                        if (sel.data.EXTRA1 == "y") 
                                            is庫房代碼為藥局 = true;
                                    }

                                    Ext.getCmp('btn_one_key_match').setHidden(!is庫房代碼為藥局);
                                    Ext.getCmp('btn_inventory_to_use_qty').setHidden(!is庫房代碼為藥局);
                                    Ext.getCmp('CB_1').setHidden(!is庫房代碼為藥局);
                                    Ext.getCmp('CB_2').setHidden(!is庫房代碼為藥局);
                                }
                            }
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            id: 'T1btn1',
                            disabled: true,
                            handler: function () {
                                msglabel('訊息區:');
                                T1F1 = '';
                                GetChkNoSt();
                                Ext.getCmp('btn_print').setDisabled(false); // 顯示【列印】按鈕
                                Ext.getCmp('btn_one_key_match').setDisabled(false); // 顯示【一鍵符合】按鈕

                                // 一鍵符合  A.所選擇庫房代碼為藥局才可看到此按鈕
                                var is庫房代碼為藥局 = false;
                                var sel = Ext.getCmp('P1').getSelection();
                                if (sel && sel.data.EXTRA1 == "y") {
                                        is庫房代碼為藥局 = true;
                                }
                                Ext.getCmp('btn_one_key_match').setHidden(!is庫房代碼為藥局);
                            }
                        },
                        {
                            xtype: 'button',
                            text: '清除',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                                Ext.getCmp('btn_print').setDisabled(true); // 隱藏【列印】按鈕
                                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                                T1Query.getForm().findField('P0').setValue(p0_default);

                                // getSetYm();
                                // T1Load();
                                T1F1 = '';
                            }
                        }, {
                            xtype: 'displayfield',
                            fieldLabel: '盤點單號',
                            padding: '0 0 0 0',
                            labelAlign: 'right',
                            id: "CHK_NO",
                            name: 'CHK_NO',
                            labelWidth: 160
                        }, {
                            name: 'CHK_STATUS',
                            xtype: 'hidden'
                        }, {
                            xtype: 'displayfield',
                            fieldLabel: '狀態',
                            padding: '0 0 0 0',
                            labelAlign: 'right',
                            name: 'CHK_STATUS_T',
                            labelWidth: 70
                        }, {
                            xtype: 'displayfield',
                            fieldLabel: '庫別分類',
                            padding: '0 0 0 8',
                            labelAlign: 'right',
                            id: 'CHK_WH_KIND',
                            name: 'CHK_WH_KIND',
                            hidden: true,
                            labelWidth: 120
                        }, {
                            xtype: 'displayfield',
                            fieldLabel: '醫院代碼',
                            padding: '0 0 0 0',
                            labelAlign: 'right',
                            id: 'HospCode',
                            name: 'HospCode',
                            hidden: true,
                            labelWidth: 120
                        }, {
                            xtype: 'displayfield',
                            fieldLabel: '盤點結束時間',
                            padding: '0 0 0 0',
                            labelAlign: 'right',
                            id: 'CAN_CHK_ENDTIME',
                            name: 'CAN_CHK_ENDTIME',
                            hidden: true,
                            labelWidth: 110
                        }, {
                            xtype: 'displayfield',
                            fieldLabel: '月結日期',
                            padding: '0 0 0 0',
                            labelAlign: 'right',
                            name: 'SET_CTIME',
                            labelWidth: 90
                        }
                    ] // end of PanelP1 items
                }, {
                    xtype: 'panel',
                    id: 'PanelP2',
                    border: false,
                    flex: 1,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'textfield',
                            fieldLabel: '院內碼',
                            name: 'MMCODE',
                            id: 'MMCODE',
                            maxLength: 13,
                            labelAlign: 'right',
                            labelWidth: 65,
                            width: 175,
                            flex: 1,
                            padding: '0 4 0 4'
                        }, {
                            xtype: 'textfield',
                            fieldLabel: '英文品名',
                            name: 'MMNAME_E',
                            id: 'MMNAME_E',
                            maxLength: 100,
                            labelAlign: 'right',
                            labelWidth: mLabelWidth,
                            flex: 1,
                            padding: '0 4 0 4'
                        }, {
                            xtype: 'textfield',
                            fieldLabel: '儲位',
                            name: 'STORE_LOC',
                            id: 'STORE_LOC',
                            maxLength: 100,
                            labelAlign: 'right',
                            labelWidth: mLabelWidth,
                            flex: 1,
                            padding: '0 4 0 4'

                        }, {
                            xtype: 'numberfield',
                            fieldLabel: '價錢(成本價)大於',
                            labelWidth: mLabelWidth * 2,
                            width:230,
                            //readOnly: true,
                            name: 'DISC_CPRICE',
                            minValue: 1,
                            //fieldCls: 'required',
                            allowBlank: true,
                            listeners: {
                                change: function (_this, newvalue, oldvalue) {
                                    //if (act_code != '' && newvalue != null) { // 參考BD0019.js
                                    //    var f = T2Form.getForm();
                                    //    var m_contprice = f.findField('M_CONTPRICE').getValue();
                                    //    // T2Form.getForm().findField('PR_AMT').setValue(Math.floor(parseInt(newvalue) * parseFloat(m_contprice))); //總金額
                                    //    T2Form.getForm().findField('TOTAL_PRICE').setValue(Math.round((parseInt(newvalue) * parseFloat(m_contprice)) * 100) / 100); //總金額
                                    //}
                                }
                            }
                        },
                        {
                            xtype: 'combo',
                            fieldLabel: '是否點滴',
                            name: 'ISIV',
                            id: 'ISIV',
                            store: st_isIV,
                            allowBlank: false, // 欄位為必填
                            width: 180,
                            queryMode: 'local',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                            flex: 1,
                            displayField: 'TEXT',
                            valueField: 'VALUE'
                        },
                    ] // end of PanelP2 items
                }, {
                    xtype: 'panel',
                    id: 'PanelP5',
                    border: false,
                    layout: 'hbox',
                    items: [
                        Ext.create('WEBAPP.form.QueryCombo', {
                            fieldLabel: '中西藥類別',
                            queryUrl: '/api/CE0041/GetDrugKindCombo',
                            extraFields: ['EXTRA1'],
                            id: 'drug_kind',
                            name: 'drug_kind',
                            labelWidth: 85,
                            flex: 1,
                            labelAlign: 'right',
                            emptyText: '全部',
                            displayField: 'TEXT',
                            padding: '0 4 0 4'
                        }),
                    ] // end of PanelP5 items
                }, {
                    xtype: 'panel',
                    id: 'PanelP3',
                    border: false,
                    flex: 1,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'checkboxfield',
                            boxLabel: '(近6個月進貨)或(近6個月醫令耗用)或(庫量<>0)或(庫存=0且無作廢)',
                            name: 'CB_1',
                            id: 'CB_1',
                            style: 'margin:0px 0px 0px 5px;',
                            hidden: true
                        }
                    ] // end of PanelP3 items
                }, {
                    xtype: 'panel',
                    id: 'PanelP4',
                    border: false,
                    flex: 1,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'checkboxfield',
                            boxLabel: '(期初庫存<f>0)或(期初=0但有進出)',
                            name: 'CB_2',
                            id: 'CB_2',
                            style: 'margin:0px 0px 0px 5px;',
                            hidden: true
                        }
                    ] // end of PanelP3 items
                }
            ]
        }]
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'STORE_LOC', type: 'string' }, // 儲位
            { name: 'STORE_QTYC', type: 'string' }, // 電腦量
            { name: 'CHK_QTY', type: 'string' }, // 盤點量
            { name: 'ALTERED_USE_QTY', type: 'string' }, // 調整消耗
            { name: 'CHK_REMARK', type: 'string' }, // 備註
            { name: 'INVENTORY', type: 'string' }, // 差異量
            { name: 'DIFF_PRICE', type: 'string' }, // 差異金額
            { name: 'THEORY_QTY', type: 'string' }, // 盤點理論值
            { name: 'PYM_INV_QTY', type: 'string' }, // 上期結存
            { name: 'APL_INQTY', type: 'string' }, // 進貨/撥發入
            { name: 'APL_OUTQTY', type: 'string' }, // 撥發出
            { name: 'TRN_INQTY', type: 'string' }, // 調撥入
            { name: 'TRN_OUTQTY', type: 'string' }, // 調撥出
            { name: 'ADJ_INQTY', type: 'string' }, // 調帳入
            { name: 'ADJ_OUTQTY', type: 'string' }, // 調帳出
            { name: 'BAK_INQTY', type: 'string' }, // 繳回入
            { name: 'BAK_OUTQTY', type: 'string' }, // 繳回出
            { name: 'REJ_OUTQTY', type: 'string' }, // 退貨量
            { name: 'DIS_OUTQTY', type: 'string' }, // 報廢量
            { name: 'EXG_INQTY', type: 'string' }, // 換貨入
            { name: 'EXG_OUTQTY', type: 'string' }, // 換貨出
            { name: 'USE_QTY', type: 'string' }, // 消耗量
            { name: 'DISC_CPRICE', type: 'string' }, // 優惠後單價
            { name: 'CONSUME_QTY', type: 'string' }, // 醫令扣庫
            { name: 'STATUS_INI', type: 'string' }, // 狀態_畫面不顯示
            { name: 'CHK_UID_NAME', type: 'string' },
            { name: 'CHK_TIME', type: 'string' },
            { name: 'HID_ALTERED_USE_QTY', type: 'string' }, // 調整消耗(**「調整消耗」僅藥局可看到 (藥局：chk_wh_kind = '0' and chk_wh_grade = '2'))
            { name: 'WH_NO', type: 'string' }, // 庫房代碼,
            { name: 'G34_MAX_APPQTY', type: 'string' }, // 單位請領基準量,
            { name: 'EST_APPQTY', type: 'string' }, // 下月預估申請量
            { name: 'ENDL', type: 'string' },
            { name: 'CHK_TIME_STRING', type: 'string' },
            { name: 'BASE_QTY_45', type: 'string' } //45天基準量 (805專用)
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 50, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST'
            },
            url: '/api/CE0041/AllM',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    chk_ym: T1Query.getForm().findField('P0').getValue(), // 盤點年月 
                    wh_no: T1Query.getForm().findField('P1').getValue(), // 庫房代碼
                    chk_no: T1Query.getForm().findField('CHK_NO').getValue(), // 盤點單號
                    mmcode: T1Query.getForm().findField('MMCODE').getValue(), // 院內碼
                    mmname_e: T1Query.getForm().findField('MMNAME_E').getValue(), // 英文品名
                    store_loc: T1Query.getForm().findField('STORE_LOC').getValue(), // 儲位,
                    disc_cprice: T1Query.getForm().findField('DISC_CPRICE').getValue(), // 價錢(成本價)大於
                    isIV: T1Query.getForm().findField('ISIV').getValue(), // 是否點滴
                    drug_kind: T1Query.getForm().findField('drug_kind').getValue(),
                    CB_1: T1Query.getForm().findField('CB_1').getValue(), // (近6個月進貨)或(近6個月醫令耗用)或(庫量<>0)或(庫存=0且無作廢)
                    CB_2: T1Query.getForm().findField('CB_2').getValue(), // (期初庫存<>0)或(期初=0但有進出)
                    endl: ''
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, operation, eOpts) {
                setVisibleColumns();
                //console.log('資料已載入:', records);
                //if (records != null) {
                //    for (var i = 0; i < records.length; i++) {
                //        var r = records[i];

                //        //if (
                //        //    !isNaN(r.data.INVENTORY) && // 差異量(inventory)不為0，整筆資料以紅字顯示
                //        //    (parseInt(r.data.INVENTORY, 10) > 0)
                //        //) {
                //        //    var view = T1Grid.getView();
                //        //    view.addCls('red-column', 1);
                //        //}
                //    } // end of for (var i = 0; i < records.length; i++) {
                //} // end of if (records != null)

            } // end of load
        }
    });
    // 設定欄位是否顯示
    var setVisibleColumns = function () {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        Ext.Ajax.request({
            url: '/api/CE0041/GetWhNoData',
            params: {
                p0: session['Inid'] // 盤點年月 
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    T1Grid.suspendLayouts();
                    for (var i = 1; i < T1Grid.columns.length; i++) {
                        if (T1Grid.columns[i].showOption == '0') {//藥局登入才能顯示的欄位
                            if (data.etts[0].WH_KIND == "0" && data.etts[0].WH_GRADE == "2") {//藥局登入才顯示欄位
                                T1Grid.columns[i].setVisible(true);
                            }
                            else {
                                T1Grid.columns[i].setVisible(false);
                            }
                        }
                    }
                    T1Grid.resumeLayouts(true);
                    myMask.hide();
                }
            },
            failure: function (response, options) {

            }
        });

    };
    function T1Load() {
        if (Ext.getCmp('HospCode').getValue() == '805') {
            Ext.getCmp('btn_add_chk_mmcode').setDisabled(false);
            Ext.getCmp('btn_add_chk_mmcode').setHidden(false);
        }
        if (T1Query.getForm().findField('CHK_STATUS').getValue() == "1") {
            Ext.getCmp('getexport').setDisabled(false);
            Ext.getCmp('export').setDisabled(false);
            Ext.getCmp('update').setDisabled(false);
            Ext.getCmp('btn_add_chk_mmcode').setDisabled(false);
            Ext.getCmp('btn_inventory_to_use_qty').setDisabled(false);
        }
        else {
            Ext.getCmp('getexport').setDisabled(true);
            Ext.getCmp('export').setDisabled(true);
            Ext.getCmp('update').setDisabled(true);
            Ext.getCmp('btn_add_chk_mmcode').setDisabled(true);
            Ext.getCmp('btn_inventory_to_use_qty').setDisabled(true);
        }

        // 編輯時間大於 關帳時間 disable 全部
        if (Ext.getCmp('CAN_CHK_ENDTIME').getValue() === 'n') {
            Ext.getCmp('getexport').setDisabled(true);
            Ext.getCmp('export').setDisabled(true);
            Ext.getCmp('update').setDisabled(true);
            Ext.getCmp('btn_print').setDisabled(true);
            Ext.getCmp('btn_one_key_match').setDisabled(true);
            Ext.getCmp('btn_add_chk_mmcode').setDisabled(true);
            Ext.getCmp('btn_add_chk_mmcode').setDisabled(true);
            Ext.getCmp('btn_add_chk_mmcode').setHidden(true);
            Ext.getCmp('btn_inventory_to_use_qty').setDisabled(true);
        }
        T1Store.load({
            params: {
                start: 0
            }
        });
        T1Tool.moveFirst();
    }

    var win4;
    var win4Form = Ext.create('Ext.form.Panel', {
        layout: 'anchor',
        defaults: {
            anchor: '100%'
        },
        fieldDefaults: {
            labelWidth: 80,
            labelAlign: "left",
            flex: 1,
            margin: 5
        },
        items: [
            Ext.create('WEBAPP.form.MMCodeCombo', {
                id: 'win4MMCodeCombo',
                name: 'win4MMCodeCombo',
                fieldLabel: '院內碼(品項名稱)',
                labelAlign: 'right',
                labelWidth: 100,
                width: 300,
                limit: 200, //限制一次最多顯示10筆
                allowBlank: false,
                queryUrl: '/api/CE0041/GetMMCODECombo', //指定查詢的Controller路徑
                extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
                getDefaultParams: function () { //查詢時Controller固定會收到的參數
                    return {
                        mat_class: T1Query.getForm().findField('P0').getValue(),  //P0:預設是動態MMCODE,
                        chk_wh_kind: T1Query.getForm().findField('CHK_WH_KIND').getValue(),
                        chk_no: T1Query.getForm().findField('CHK_NO').getValue()
                    };
                },
                listeners: {
                    select: function (c, r, i, e) {
                        //選取下拉項目時，顯示回傳值
                    }
                }
            })
        ]
    });
    var win4CreateCHK_DETAIL = function () {
        myMask.show();
        var mmcode = Ext.getCmp('win4MMCodeCombo').getValue();
        var ym = Ext.getCmp('P0').getValue();
        var wh_no = Ext.getCmp('P1').getValue();
        var chk_no = Ext.getCmp('CHK_NO').getValue();
        var chk_wh_kind = Ext.getCmp('CHK_WH_KIND').getValue();

        Ext.Ajax.request({
            url: '/api/CE0041/CreateMMCodeChk',
            method: reqVal_p,
            params: {
                mmcode: mmcode,
                ym: ym,
                wh_no: wh_no,
                chk_no: chk_no,
                chk_wh_kind: chk_wh_kind
            },
            success: function (response) {
                myMask.hide();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    Ext.Msg.alert('', '院內碼建立成功');
                    T1Load();
                }
                else {
                    myMask.hide();
                    Ext.Msg.alert('', data.msg);
                }
            },
            failure: function (response) {
                myMask.hide();
                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
            }
        });
    };
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                id: 'getexport',
                itemId: 'getexport',
                text: '匯入',
                disabled: true,
                handler: function () {
                    msglabel('訊息區:');
                    showWin3();
                }
            },
            {
                id: 'export',
                itemId: 'export',
                text: '匯出',
                disabled: true,
                handler: function () {
                    var param = new Array();
                    if (T1Store.getCount() > 0) {
                        //param.push({ name: 'CHK_NO', value: T1F1 });
                        param.push({ name: 'CHK_NO', value: T1Query.getForm().findField('CHK_NO').getValue() }); // 盤點單號
                        param.push({ name: 'CHK_YM', value: T1Query.getForm().findField('P0').getValue() }); // 
                        param.push({ name: 'CHK_WH_NO', value: T1Query.getForm().findField('P1').getValue() }); // 
                        param.push({ name: 'MMCODE', value: T1Query.getForm().findField('MMCODE').getValue() }); // 院內碼
                        param.push({ name: 'MMNAME_E', value: T1Query.getForm().findField('MMNAME_E').getValue() }); // 英文品名
                        param.push({ name: 'STORE_LOC', value: T1Query.getForm().findField('STORE_LOC').getValue() }); // 儲位
                        param.push({ name: 'DISC_CPRICE', value: T1Query.getForm().findField('DISC_CPRICE').getValue() }); // 價錢(成本價)大於
                        param.push({ name: 'ISIV', value: T1Query.getForm().findField('ISIV').getValue() }); // 是否點滴
                        param.push({ name: 'CB_1', value: T1Query.getForm().findField('CB_1').getValue() }); // checkbox 1
                        param.push({ name: 'CB_2', value: T1Query.getForm().findField('CB_2').getValue() }); // checkbox 2
                        PostForm(urlCE0041Excel, param); // '../../../api/CE0041/Excel'
                        msglabel('訊息區:匯出完成');
                    }
                    else {
                        Ext.Msg.alert('訊息', '無資料可匯出');
                    }
                }
            }, {
                id: 'update',
                itemId: 'update', text: '儲存', disabled: true, handler: function () {

                    var tempData = T1Grid.getStore().data.items;
                    var data = [];
                    let CHK_QTY = '';
                    let CHK_REMARK = '';

                    for (var i = 0; i < tempData.length; i++) {
                        // WH_NO=前端T1Grid.MMCODE(院內碼)
                        if (tempData[i].dirty) {
                            debugger
                            var o = tempData[i];
                            o.WH_NO = T1Query.getForm().findField('P1').getValue();
                            o.WH_NO_C = T1Query.getForm().findField('P1').getRawValue();
                            //if (tempData[i].data.CHK_QTY == '' || tempData[i].data.CHK_QTY == null) {
                            //    var msg = '';
                            //    msg = tempData[i].data.MMCODE + ' 請輸入盤點量'
                            //    Ext.Msg.alert('提示', msg);
                            //    return;
                            //}

                            // T1Query.getForm().findField('P1').getValue(); // 庫房代碼(WH_NO)
                            data.push(tempData[i].data);
                            //break;
                        }
                    }

                    var filteredData = data.map(function (item) {
                        var filteredItem = {
                            CHK_NO: item.CHK_NO,
                            CHK_QTY: item.CHK_QTY,
                            ALTERED_USE_QTY: item.ALTERED_USE_QTY,
                            EST_APPQTY: item.EST_APPQTY,
                            G34_MAX_APPQTY: item.G34_MAX_APPQTY,
                            CHK_REMARK: item.CHK_REMARK,
                            MMCODE: item.MMCODE,
                            WH_NO_C: item.WH_NO_C,
                            ORI_CHK_QTY: item.ORI_CHK_QTY,
                            STORE_QTYC: item.STORE_QTYC,
                            CHK_WH_NO: item.WH_NO,
                            CHK_YM: item.CHK_YM,
                            CHK_WH_GRADE: item.CHK_WH_GRADE,
                            CHK_WH_KIND: item.CHK_WH_KIND,
                        };
                        return filteredItem;
                    });

                    myMask.show();

                    var url = '../../../api/CE0041/UpdateDetails';
                    Ext.Ajax.request({
                        url: url,
                        method: reqVal_p,
                        contentType: "application/json",
                        timeout: 0,
                        params: {
                            list: Ext.util.JSON.encode(filteredData)
                        },
                        success: function (response) {
                            myMask.hide();
                            var data = Ext.decode(response.responseText);
                            if (data.success) {

                                var data = JSON.parse(response.responseText);
                                if (data.success == false) {
                                    Ext.Msg.alert('失敗', data.msg);
                                    return;
                                }

                                msglabel('訊息區:資料更新成功');
                                T1Store.load({
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
            }, {
                id: 'btn_print',
                itemId: 'item_print', text: '列印', disabled: true, handler: function () {

                    var tempData = T1Grid.getStore().data.items;
                    var data = [];
                    let CHK_QTY = '';
                    let CHK_REMARK = '';
                    if (tempData.length > 0)
                        T1LastRec = tempData[0];
                    if (T1LastRec == null)
                        return;
                    for (var i = 0; i < tempData.length; i++) {
                        // WH_NO=前端T1Grid.MMCODE(院內碼)
                        if (tempData[i].dirty) {
                            var o = tempData[i];
                            o.WH_NO = T1Query.getForm().findField('P1').getValue();
                            o.WH_NO_C = T1Query.getForm().findField('P1').getRawValue();
                            //console.log("CHK_NO(盤點單號=" + o.CHK_NO);
                            //console.log("CHK_QTY(盤點量=" + o.CHK_QTY);
                            //console.log("ALTERED_USE_QTY(調整消耗=" + o.ALTERED_USE_QTY);
                            //console.log("CHK_REMARK(備註=" + o.CHK_REMARK);
                            //console.log("INVENTORY(差異量=" + o.INVENTORY);
                            //console.log("WH_NO(庫房代號=" + T1Query.getForm().findField('P1').getValue());
                            //console.log("WH_NO_C(庫房名稱=" + T1Query.getForm().findField('P1').getRawValue());
                            //console.log("MMCODE(院內碼=" + o.MMCODE);

                            //if (tempData[i].data.CHK_QTY == '' || tempData[i].data.CHK_QTY == null) {
                            //    var msg = '';
                            //    msg = tempData[i].data.MMCODE + ' 請輸入盤點量'
                            //    Ext.Msg.alert('提示', msg);
                            //    return;
                            //}

                            // T1Query.getForm().findField('P1').getValue(); // 庫房代碼(WH_NO)
                            data.push(tempData[i].data);
                            break;
                        }
                    }

                    myMask.show();

                    //chkno: T1Query.getForm().findField('CHK_NO').getValue(), // 盤點單號
                    //mmname_e: T1Query.getForm().findField('MMNAME_E').getValue(), // 英文品名
                    //store_loc: T1Query.getForm().findField('STORE_LOC').getValue(), // 儲位,
                    //disc_cprice: T1Query.getForm().findField('DISC_CPRICE').getValue(), // 價錢(成本價)大於


                    //console.log("T1LastRec.data.CHK_NO=" + T1LastRec.data.CHK_NO);
                    showReport('/Report/C/CE0002.aspx', T1LastRec.data.CHK_NO, "");
                }
            }, {
                id: 'btn_one_key_match',
                itemId: 'item_one_key_match', text: '一鍵符合',
                hidden: true, // (5)一鍵符合, A.所選擇庫房代碼為藥局才可看到此按鈕(chk_wh_kind = '0' and chk_wh_grade = '2') 
                disabled: true,
                handler: function () {

                    var tempData = T1Grid.getStore().data.items;
                    var data = [];
                    let CHK_QTY = '';
                    let CHK_REMARK = '';

                    for (var i = 0; i < tempData.length; i++) {
                        // WH_NO=前端T1Grid.MMCODE(院內碼)
                        // if (tempData[i].dirty) {
                        var o = tempData[i];
                        o.WH_NO = T1Query.getForm().findField('P1').getValue();
                        o.WH_NO_C = T1Query.getForm().findField('P1').getRawValue();
                        //console.log("CHK_NO(盤點單號=" + o.CHK_NO);
                        //console.log("CHK_QTY(盤點量=" + o.CHK_QTY);
                        //console.log("ALTERED_USE_QTY(調整消耗=" + o.ALTERED_USE_QTY);
                        //console.log("CHK_REMARK(備註=" + o.CHK_REMARK);
                        //console.log("INVENTORY(差異量=" + o.INVENTORY);
                        //console.log("WH_NO(庫房代號=" + T1Query.getForm().findField('P1').getValue());
                        //console.log("WH_NO_C(庫房名稱=" + T1Query.getForm().findField('P1').getRawValue());
                        //console.log("MMCODE(院內碼=" + o.MMCODE);

                        //if (tempData[i].data.CHK_QTY == '' || tempData[i].data.CHK_QTY == null) {
                        //    var msg = '';
                        //    msg = tempData[i].data.MMCODE + ' 請輸入盤點量'
                        //    Ext.Msg.alert('提示', msg);
                        //    return;
                        //}

                        // T1Query.getForm().findField('P1').getValue(); // 庫房代碼(WH_NO)
                        data.push(tempData[i].data);
                        // }
                    }

                    var filteredData = data.map(function (item) {
                        var filteredItem = {
                            CHK_NO: item.CHK_NO
                        };
                        return filteredItem;
                    });


                    myMask.show();
                    var url = '../../../api/CE0041/BtnOneKeyMatch';

                    Ext.Ajax.request({
                        url: url,
                        method: reqVal_p,
                        contentType: "application/json",
                        params: {
                            list: Ext.util.JSON.encode(filteredData)
                        },
                        success: function (response) {

                            myMask.hide();
                            var data = Ext.decode(response.responseText);
                            if (data.success) {

                                var data = JSON.parse(response.responseText);
                                if (data.success == false) {
                                    Ext.Msg.alert('失敗', data.msg);
                                    return;
                                }

                                msglabel('訊息區:資料更新成功');
                                T1Store.load({
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
            }, {
                id: 'btn_add_chk_mmcode',
                itemId: 'btn_add_chk_mmcode',
                text: '新增院內碼',
                hidden: true, // 只有花蓮能看到
                disabled: true,
                handler: function () {
                    if (!win4) {
                        win4 = Ext.create("widget.window", {
                            title: "新增院內碼",
                            width: 350,
                            height: 300,
                            layout: "fit",
                            modal: true,
                            closable: true,
                            closeAction: 'hide',
                            items: [
                                win4Form
                            ],
                            buttons: [
                                {
                                    text: "確定", handler: function () {
                                        if (win4Form.getForm().isValid()) {
                                            win4CreateCHK_DETAIL();
                                        } else {
                                            Ext.Msg.alert('提醒', '輸入資料格式有誤');
                                            msglabel('訊息區:輸入資料格式有誤');
                                        }
                                        this.up("window").close();
                                    }
                                },
                                { text: "取消", handler: function () { this.up("window").close(); } }
                            ],
                            listeners: {
                                show: function (self, eOpts) {
                                    win4Form.getForm().findField('win4MMCodeCombo').setValue('');
                                }
                            }
                        });
                    }

                    if (win4.isVisible()) {
                        win4.hide();
                    } else {
                        win4.show();
                    }

                }
            }, {
                id: 'btn_inventory_to_use_qty',
                itemId: 'btn_inventory_to_use_qty',
                text: '差異轉消耗',
                disabled: false,
                hidden: true,
                handler: function () {
                    Ext.MessageBox.confirm('差異轉消耗', '是否將差異量轉為消耗量?', function (btn, text) {
                        if (btn === 'yes') {
                            Ext.Ajax.request({
                                url: '/api/CE0041/UpdateGapToUse',
                                method: reqVal_p,
                                params: {
                                    chk_no: T1Query.getForm().findField('CHK_NO').getValue()
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        msglabel('差異轉消耗完成');
                                        T1Load();
                                    } else {
                                        Ext.MessageBox.alert('錯誤', '差異轉消耗完成失敗');
                                    }
                                },
                                failure: function (response) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                }
                            });
                        }
                    }
                    );

                   
                },
            }, {
                xtype: 'displayfield',
                fieldLabel: '',
                value: '<span style="color:red">手動輸入盤點量請一頁儲存一次</span>',
                padding: '0 0 0 20'
            }
        ]
    });
    var findColumnIndex = function (columns, dataIndex) {
        var index;
        for (index = 0; index < columns.length; ++index) {
            if (columns[index].dataIndex == dataIndex) { break; }
        }
        return index == columns.length ? -1 : index;
    };
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
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
            }, {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        enableKeyEvents: true,
        // -- grid查詢結果(一頁50筆資料，移除checkbox勾選欄位)
        //selModel: Ext.create('Ext.selection.CheckboxModel', {
        //    checkOnly: true,
        //    injectCheckbox: 'first',
        //    mode: 'SIMPLE',
        //    showHeaderCheckbox: true
        //}),
        // -- grid查詢結果(一頁50筆資料，移除checkbox勾選欄位) 結束
        columns: [
            {
                xtype: 'rownumberer'
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 120,
                //                locked: true,
                renderer: drawCellColor()
            }, {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 120,
                //locked: true,
                renderer: drawCellColor()
            }, {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 120,
                //locked: true,
                renderer: drawCellColor()
            }, {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 80,
                renderer: drawCellColor()
            }, {
                text: "儲位",
                dataIndex: 'STORE_LOC',
                width: 80,
                renderer: drawCellColor()
            }, {
                text: "電腦量",
                dataIndex: 'STORE_QTYC',
                style: 'text-align:left',
                width: 80, align: 'right',
                renderer: drawCellColor()
            }, {
                text: "盤點量",
                style: 'text-align:left; color:red',
                align: 'right',
                dataIndex: 'CHK_QTY',
                width: 70,
                editor: {
                    xtype: 'textfield',
                    regexText: '只能輸入數字',
                    regex: /^-?[0-9]\d*(\.\d+)?$/, // 用正規表示式限制可輸入內容
                    selectOnFocus: true,
                    listeners: {
                        change: function (field, newVal, oldVal) {
                            if (T1F2 != "" && newVal == "") {
                                field.setValue(oldVal);
                                Ext.MessageBox.alert('提示', '盤點量不可更新為空');
                                return;
                            }
                        }
                    }
                },
                renderer: drawCellColor()
            }, {
                text: "調整消耗",
                style: 'text-align:left; color:red',
                align: 'right',
                dataIndex: 'ALTERED_USE_QTY',
                width: 90,
                hidden: true, // 預設為隱藏, **「調整消耗」僅藥局可看到 (藥局：chk_wh_kind = '0' and chk_wh_grade = '2')
                editor: {
                    xtype: 'textfield',
                    regexText: '只能輸入數字',
                    regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
                    selectOnFocus: true,
                    listeners: {
                        change: function (field, newVal, oldVal) {
                            if (T1F2 != "" && newVal == "") {
                                field.setValue(oldVal);
                                Ext.MessageBox.alert('提示', '調整消耗 不可更新為空');
                                return;
                            }
                        }
                    }
                },
                renderer: drawCellColor()
            }, {
                text: "單位請領基準量",
                style: 'text-align:left; color:red',
                hidden: true,
                align: 'right',
                dataIndex: 'G34_MAX_APPQTY',
                width: 150,
                editor: {
                    xtype: 'textfield',
                    regexText: '只能輸入數字',
                    regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
                    selectOnFocus: true,
                    listeners: {
                        change: function (field, newVal, oldVal) {
                            if (T1F2 != "" && newVal == "") {
                                field.setValue(oldVal);
                                Ext.MessageBox.alert('提示', '單位請領基準量 不可更新為空');
                                return;
                            }
                        }
                    }
                },
                renderer: drawCellColor()
            }, {
                text: "下月預估申請量",
                style: 'text-align:left; color:red',
                hidden: true,
                align: 'right',
                dataIndex: 'EST_APPQTY',
                width: 150,
                editor: {
                    xtype: 'textfield',
                    regexText: '只能輸入數字',
                    regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
                    selectOnFocus: true,
                    listeners: {
                        change: function (field, newVal, oldVal) {
                            if (T1F2 != "" && newVal == "") {
                                field.setValue(oldVal);
                                Ext.MessageBox.alert('提示', '下月預估申請量 不可更新為空');
                                return;
                            }
                        }
                    }
                },
                renderer: drawCellColor()
            }, {
                text: "備註",
                style: 'text-align:left; color:red',
                dataIndex: 'CHK_REMARK',
                width: 140,
                editor: {
                    xtype: 'textfield',
                    selectOnFocus: true,
                    fieldStyle: 'text-align:left;',
                    listeners: {

                    }
                },
                renderer: drawCellColor()
            }, {
                text: "差異量",
                align: 'right',
                dataIndex: 'INVENTORY',
                width: 120,
                renderer: drawCellColor()
            }, {
                text: "差異金額",
                dataIndex: 'DIFF_PRICE',
                width: 120,
                renderer: drawCellColor()
            }, {
                text: "盤點理論值",
                dataIndex: 'THEORY_QTY',
                align: 'right',
                style: 'text-align:left',
                width: 80,
                renderer: drawCellColor(),
            }, {
                text: "上期結存",
                dataIndex: 'PYM_INV_QTY',
                align: 'right',
                style: 'text-align:left',
                width: 80,
                renderer: drawCellColor()
            }, {
                text: "進貨/撥發入",
                dataIndex: 'APL_INQTY',
                align: 'right',
                style: 'text-align:left',
                width: 100,
                renderer: drawCellColor()
            }, {
                text: "撥發出",
                dataIndex: 'APL_OUTQTY',
                align: 'right',
                style: 'text-align:left',
                width: 70,
                renderer: drawCellColor()
            }, {
                text: "調撥入",
                dataIndex: 'TRN_INQTY',
                align: 'right',
                style: 'text-align:left',
                width: 70,
                renderer: drawCellColor()
            }, {
                text: "調撥出",
                dataIndex: 'TRN_OUTQTY',
                align: 'right',
                style: 'text-align:left',
                width: 70,
                renderer: drawCellColor()
            }, {
                text: "調帳入",
                dataIndex: 'ADJ_INQTY',
                align: 'right',
                style: 'text-align:left',
                width: 70,
                renderer: drawCellColor()
            }, {
                text: "調帳出",
                dataIndex: 'ADJ_OUTQTY',
                align: 'right',
                style: 'text-align:left',
                width: 70,
                renderer: drawCellColor()
            }, {
                text: "繳回入",
                dataIndex: 'BAK_INQTY',
                align: 'right',
                style: 'text-align:left',
                width: 70,
                renderer: drawCellColor()
            }, {
                text: "繳回出",
                dataIndex: 'BAK_OUTQTY',
                align: 'right',
                style: 'text-align:left',
                width: 70,
                renderer: drawCellColor()
            }, {
                text: "退貨量",
                dataIndex: 'REJ_OUTQTY',
                align: 'right',
                style: 'text-align:left',
                width: 70,
                renderer: drawCellColor()
            }, {
                text: "報廢量",
                dataIndex: 'DIS_OUTQTY',
                align: 'right',
                style: 'text-align:left',
                width: 70,
                renderer: drawCellColor()
            }, {
                text: "換貨入",
                dataIndex: 'EXG_INQTY',
                align: 'right',
                style: 'text-align:left',
                width: 70,
                renderer: drawCellColor()
            }, {
                text: "換貨出",
                dataIndex: 'EXG_OUTQTY',
                align: 'right',
                style: 'text-align:left',
                width: 70,
                renderer: drawCellColor()
            }, {
                text: "消耗量",
                dataIndex: 'USE_QTY',
                align: 'right',
                style: 'text-align:left',
                width: 70,
                renderer: drawCellColor()
            }, {
                id: 'BASE_QTY_45',
                itemId: 'BASE_QTY_45',
                text: "45天基準量",
                dataIndex: 'BASE_QTY_45',
                align: 'right',
                style: 'text-align:left',
                width: 100,
                renderer: drawCellColor()
            }, {
                text: "優惠後單價",
                dataIndex: 'DISC_CPRICE',
                align: 'right',
                style: 'text-align:left',
                width: 100,
                renderer: drawCellColor()
            }, {
                text: "醫令扣庫",
                dataIndex: 'CONSUME_QTY',
                align: 'right',
                style: 'text-align:left',
                width: 80,
                renderer: drawCellColor()
            }, {
                text: "盤點人員",
                dataIndex: 'CHK_UID_NAME',
                width: 80,
                renderer: drawCellColor()
            }, {
                text: "盤點時間",
                dataIndex: 'CHK_TIME_STRING',
                width: 150,
                renderer: drawCellColor()
            },
            {
                dataIndex: 'CHK_NO',
                hidden: true
            },
            {
                dataIndex: 'STORE_QTYC',
                hidden: true
            }, {
                dataIndex: 'STATUS_INI', // 狀態_畫面不顯示
                hidden: true
            }, {
                dataIndex: 'HID_ALTERED_USE_QTY',
                hidden: true
            }, {
                dataIndex: 'ORI_CHK_QTY',
                hidden: true
            }, {
                dataIndex: 'CHK_YM',
                hidden: true
            }, {
                dataIndex: 'CHK_WH_GRADE',
                hidden: true
            }, {
                dataIndex: 'CHK_WH_KIND',
                hidden: true
            }, {
                header: "",
                flex: 1
            }],
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1
            })
        ],
        listeners: {
            beforeedit: function (editor, e) {
                if (e.field == 'EST_APPQTY' || e.field == 'G34_MAX_APPQTY') {
                    return true;
                }
                T1F2 = e.record.get('CHK_UID_NAME');
                if (e.record.get('CHK_STATUS') != '1') {
                    return false;
                }

                if (Ext.getCmp('CAN_CHK_ENDTIME').getValue() === 'n') {
                    return false;
                }

                //if (isEditable(T1LastRec.data.CHK_YM) == false) {
                //    return false;
                //}
                
                //var editColumnIndex1 = findColumnIndex(T1Grid.columns, 'CHK_QTY');
                //var editColumnIndex2 = findColumnIndex(T1Grid.columns, 'EST_APPQTY');
                //var editColumnIndex3 = findColumnIndex(T1Grid.columns, 'G34_MAX_APPQTY');
                //var editColumnIndex4 = findColumnIndex(T1Grid.columns, 'ALTERED_USE_QTY');
                //// STATUS_INI不是1 則不可填寫
                //if (e.colIdx+3 != editColumnIndex1 && e.colIdx+3 != editColumnIndex2 && e.colIdx+3 != editColumnIndex3 && e.colIdx+3 != editColumnIndex4
                //    && e.record.get('STATUS_INI') != '1') {
                //    return false;
                //}
            },
            selectionchange: function (model, records) {
                //msglabel('訊息區:');
                T1Rec = records.length;
                T1LastRec = records[0];
            },
            load: function (store, records, successful, operation, eOpts) {
                //if (records.length>0)
                //    T1LastRec = records[0];
            }
        },
        viewConfig: {
            listeners: {
                itemkeydown: function (grid, rec, item, idx, e) {
                    if (e.keyCode == 38) { //上
                        e.preventDefault();
                        var editPlugin = this.up().editingPlugin;
                        editPlugin.startEdit(editPlugin.context.rowIdx - 1, editPlugin.context.colIdx);
                    } else if (e.keyCode == 40) { //下
                        e.preventDefault();
                        var editPlugin = this.up().editingPlugin;
                        editPlugin.startEdit(editPlugin.context.rowIdx + 1, editPlugin.context.colIdx);

                    } else if (e.keyCode == 37) { //左
                        e.preventDefault();
                        var editPlugin = this.up().editingPlugin;
                        editPlugin.startEdit(editPlugin.context.rowIdx, editPlugin.context.colIdx - 1);

                    } else if (e.keyCode == 39) { //右
                        e.preventDefault();
                        var editPlugin = this.up().editingPlugin;
                        editPlugin.startEdit(editPlugin.context.rowIdx, editPlugin.context.colIdx + 1);

                    }

                }
            }
        }
    });

    Ext.define('T3Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'CHK_NO', type: 'string' },
            { name: 'CHECK_RESULT', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'WH_NO', type: 'string' },
            { name: 'WH_NO_C', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'STORE_LOC', type: 'string' }, // 儲位
            { name: 'STORE_QTYC', type: 'string' }, // 電腦量
            { name: 'CHK_QTY', type: 'string' }, // 盤點量
            { name: 'ALTERED_USE_QTY', type: 'string' }, // 調整消耗
            { name: 'CHK_REMARK', type: 'string' }, // 備註
            { name: 'INVENTORY', type: 'string' }, // 差異量
            { name: 'DIFF_PRICE', type: 'string' }, // 差異金額
            { name: 'PYM_INV_QTY', type: 'string' }, // 上期結存
            { name: 'APL_INQTY', type: 'string' }, // 進貨/撥發入
            { name: 'APL_OUTQTY', type: 'string' }, // 撥發出
            { name: 'TRN_INQTY', type: 'string' }, // 調撥入
            { name: 'TRN_OUTQTY', type: 'string' }, // 調撥出
            { name: 'ADJ_INQTY', type: 'string' }, // 調帳入
            { name: 'ADJ_OUTQTY', type: 'string' }, // 調帳出
            { name: 'BAK_INQTY', type: 'string' }, // 繳回入
            { name: 'BAK_OUTQTY', type: 'string' }, // 繳回出
            { name: 'REJ_OUTQTY', type: 'string' }, // 退貨量
            { name: 'DIS_OUTQTY', type: 'string' }, // 報廢量
            { name: 'EXG_INQTY', type: 'string' }, // 換貨入
            { name: 'EXG_OUTQTY', type: 'string' }, // 換貨出
            { name: 'USE_QTY', type: 'string' }, // 消耗量
            { name: 'DISC_CPRICE', type: 'string' }, // 優惠後單價
            { name: 'CONSUME_QTY', type: 'string' }, // 醫令扣庫
            { name: 'STATUS_INI', type: 'string' }, // 狀態_畫面不顯示
            { name: 'CHK_UID_NAME', type: 'string' },
            { name: 'CHK_TIME', type: 'string' },
            { name: 'HID_ALTERED_USE_QTY', type: 'string' }, // 調整消耗(**「調整消耗」僅藥局可看到 (藥局：chk_wh_kind = '0' and chk_wh_grade = '2'))
            { name: 'WH_NO', type: 'string' }, // 庫房代碼,
            { name: 'CHECK_RESULT', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'EST_APPQTY', type: 'string' }, // 下月預估申請量
            { name: 'G34_MAX_APPQTY', type: 'string' }, // 單位請領基準量
            { name: 'ENDL', type: 'string' },
            { name: 'BASE_QTY_45', type: 'string' } //45天基準量 (805專用)
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
            url: '/api/CE0041/T3Store', // ★
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });

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
            /*{
                xtype: 'button',
                id: 'T3export',
                name: 'T3export',
                text: '下載匯入範本', handler: function () {
                    var p = new Array();
                    p.push({ name: 'p0', value: T1F1 });
                    PostForm(T3GetExcel, p);
                    msglabel('訊息區:匯出完成');
                }
            },*/
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
                            var mask = new Ext.LoadMask(T3Grid, { msg: '處理中...' });
                            mask.show();
                            var formData = new FormData();
                            formData.append("file", f);
                            formData.append("CHK_NO", T1F1);
                            formData.append("WH_NO", T1Query.getForm().findField('P1').getValue()); // 庫房代碼(供應中心)
                            var ajaxRequest = $.ajax({
                                type: "POST",
                                url: "/api/CE0041/SendExcel",
                                data: formData,
                                timeout: 1200000,
                                processData: false,
                                //必須false才會自動加上正確的Content-Type
                                contentType: false,
                                success: function (data, textStatus, jqXHR) {
                                    if (!data.success) {
                                        T3Store.removeAll();
                                        Ext.MessageBox.alert("提示", data.msg);
                                        msglabel("訊息區:");
                                        Ext.getCmp('T3insert').setDisabled(true);
                                    }
                                    else {
                                        msglabel("訊息區:檔案讀取成功");

                                        T3Store.loadData(data.etts, false);
                                        if (data.msg == "True") {
                                            Ext.getCmp('T3insert').setDisabled(false);
                                            Ext.MessageBox.alert("提示", "檢核<span style=\"color: blue; font-weight: bold\">成功</span>，可進行更新動作。");
                                        }
                                        if (data.msg == "False") {
                                            Ext.MessageBox.alert("提示", "檢核<span style=\"color: red; font-weight: bold\">失敗</span>，請依錯誤說明修改Excel檔。");
                                        }
                                    }
                                    Ext.getCmp('T3send').fileInputEl.dom.value = '';
                                    mask.hide();
                                },
                                error: function (jqXHR, textStatus, errorThrown) {
                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                    Ext.getCmp('T3send').fileInputEl.dom.value = '';
                                    Ext.getCmp('T3insert').setDisabled(true);
                                    mask.hide();
                                }
                            }); // end of var ajaxRequest = $.ajax({
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
            xtype: 'rownumberer',
            width: 60,
        }, {
            text: "檢核結果",
            dataIndex: 'CHECK_RESULT',
            width: 200
        }, {
            text: "盤點單號",
            dataIndex: 'CHK_NO',
            width: 130
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100,
            sortable: true
        }, {
            text: "盤點數量",
            dataIndex: 'CHK_QTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "單位請領基準量",
            dataIndex: 'G34_MAX_APPQTY',
            style: 'text-align:left',
            width: 150, align: 'right',
            id: 'T3Grid_G34_MAX_APPQTY'
        }, {
            text: "下月預估申請量",
            dataIndex: 'EST_APPQTY',
            style: 'text-align:left',
            width: 150, align: 'right',
            id: 'T3Grid_EST_APPQTY'
        }, {
            text: "調整消耗",
            dataIndex: 'ALTERED_USE_QTY',
            style: 'text-align:left',
            width: 80, align: 'right',
            id: 'T3Grid_ALTERED_USE_QTY'
        },
        {
            text: "備註",
            dataIndex: 'CHK_REMARK',
            style: 'text-align:left',
            width: 150
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
                        region: 'center',
                        layout: 'fit',
                        split: true,
                        collapsible: false,
                        border: false,
                        items: [T1Grid]
                    }

                ]
            }
        ]
    });

    var winActWidth = viewport.width - 10;
    var winActHeight = viewport.height - 10;
    var win3 = Ext.widget('window', {
        title: '盤點匯入檢核',
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
                var mask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                mask.show();
                Ext.Ajax.request({
                    url: '/api/CE0041/ImportUpdate',
                    method: reqVal_p,
                    params: {
                        item: Ext.encode(Ext.pluck(T3Store.data.items, 'data')),
                        CHK_NO: T1F1,
                        WH_NO: T1Query.getForm().findField('P1').getValue()
                    },
                    timeout: 0,
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
                            T1Load();
                        }
                        mask.hide();
                    },
                    failure: function (form, action) {
                        mask.hide();
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
                // myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                //myMask.hide();
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
            },
            show: function (self, eOpts) {
                Ext.getCmp('T3Grid_G34_MAX_APPQTY').hide();
                Ext.getCmp('T3Grid_EST_APPQTY').hide();
                Ext.getCmp('T3Grid_ALTERED_USE_QTY').hide();
                if (is顯示調整消耗欄位) {
                    Ext.getCmp('T3Grid_ALTERED_USE_QTY').show();
                }
                if (is花蓮且衛星庫房) {
                    Ext.getCmp('T3Grid_G34_MAX_APPQTY').show();
                    Ext.getCmp('T3Grid_EST_APPQTY').show();
                }
            }
        }
    });
    function showWin3() {
        if (win3.hidden) {
            T3Cleanup();
            T3Store.removeAll();
            win3.setTitle(T1F1 + '盤點匯入檢核');
            win3.show();
        }
    }
    function hideWin3() {
        if (!win3.hidden) {
            win3.hide();
            T3Cleanup();
        }
    }

    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
    myMask.hide();

    //getSetYm();
    //setYm();
    //讀取醫院別
    st_setym.load({
        callback: function (records, operation, success) {
            if (success) {
                var initData = records[0].getData();
                if (initData.HOSP_CODE != "805") {
                    Ext.getCmp('BASE_QTY_45').setHidden(true); //載入資料時檢查醫院是否為花蓮(控制45天基準量是否顯示)
                }
            }
        }
    });
});
